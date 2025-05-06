using UnityEngine;

namespace src.PresentationLayer
{
    /// <summary>
    /// 表现层组件,负责处理角色的实际位置更新和物理交互，限制在XY平面。
    /// 接收来自行为层或其他逻辑层的指令，并将其应用于Rigidbody。
    /// 使用Rigidbody和Collider处理碰撞、重力、跳跃和被动位移。
    /// 行走通过Rigidbody.MovePosition实现，提供直接控制感并尊重碰撞。
    /// Z轴位置将被强制固定。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterMotor : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Collider2D _collider; // 获取Collider引用，可能用于获取尺寸信息

        // --- 可配置参数 ---
        // [Header("移动配置")]
        // [SerializeField] private float defaultWalkSpeed = 5f; // 默认行走速度 (单位/秒)
        // [SerializeField] private float defaultJumpForce = 500f; // 默认跳跃冲量大小 (需要根据角色质量调整)

        [Header("物理与地面检测")]
        [SerializeField] private Transform groundCheckPoint; // 用于检测地面的参考点 (通常在角色脚底)
        [SerializeField] private float groundCheckRadius = 0.2f; // 地面检测半径
        [SerializeField] private LayerMask groundLayer; // 定义哪些层是“地面”
        [SerializeField] private float gravityMultiplier = 2.0f; // 重力乘数，用于调整跳跃手感 (基于物理设置里的重力)

        [Header("视觉朝向")]
        [Tooltip("需要翻转以改变朝向的Transform（通常是包含Sprite或模型的主体）。如果为空，则使用此GameObject的Transform。")]
        [SerializeField] private Transform visualTransform; // 用于视觉翻转的Transform
        
        // --- 运行时状态 ---
        private bool _isGrounded; // 角色当前是否在地面上
        private Vector2 _currentMovementInput = Vector2.zero; // 当前帧期望的主动移动向量 (由外部设置)
        private bool _jumpRequested; // 是否请求了跳跃
        private Vector2 _requestedJumpVelocity = Vector2.zero;
        private int _targetFacingDirection = 1;

        #region Unity生命周期

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>(); 

            // --- 初始化Rigidbody设置 ---
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;        // 冻结旋转，防止角色摔倒
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;       // 平滑视觉移动
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 最精确的碰撞检测，适合快速移动的角色
            _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;                // 确保角色始终活动
            
            visualTransform ??= transform;
            ApplyVisualFacingDirection();
        }

        private void Update()
        {
            // 每帧更新非物理相关的状态，例如地面检测
            UpdateGroundedStatus();
        }

        private void FixedUpdate()
        {
            // 在FixedUpdate中应用所有基于物理的移动和力

            // 1. 应用自定义重力 (如果需要比默认重力更强的效果)
            ApplyCustomGravity();

            // 2. 处理跳跃请求
            ApplyJumpVelocity();

            // 3. 应用主动移动 (行走/跑步)
            ApplyActiveMovement();
            
        }

        private void LateUpdate()
        {
            ApplyVisualFacingDirection();
        }

        #endregion

        #region 公开接口 (供行为层或其他逻辑调用)

        /// <summary>
        /// 强制设置角色位置。会停止当前速度并移动到目标点。
        /// 用于回合开始、重置、特殊技能（如瞬移）等场景。
        /// </summary>
        /// <param name="position">目标世界坐标 (X, Y)。</param>
        public void SetPosition(Vector2 position)
        {
            // Debug.Log($"[Movement] SetPosition: {targetPosition}");
            _rb.velocity = Vector2.zero; // 停止当前物理速度
            _rb.position = position; // 直接设置Rigidbody的位置
            _currentMovementInput = Vector2.zero; // 重置主动移动意图
        }

        /// <summary>
        /// 设置角色当前期望的主动水平移动方向和速度。
        /// 在角色处于可控移动状态时，由行为层在Update或FixedUpdate中持续调用。
        /// </summary>
        /// <param name="horizontalDirection">-1 (左), 0 (停止), 1 (右)。这是绝对X轴方向。</param>
        /// <param name="speed">移动速度 (若小于0，则使用默认速度)。</param>
        public void SetHorizontalMovement(float horizontalDirection, float speed)
        {
            // 只设置X轴移动意图，Y轴由跳跃和重力控制
            _currentMovementInput.x = horizontalDirection * speed;
            // Debug.Log($"[Movement] SetHorizontalMovement: Dir={horizontalDirection}, Speed={speed}, InputX={currentMovementInput.x}");
        }

        /// <summary>
        /// 请求执行一次跳跃，并指定起跳速度。
        /// 实际的速度设置会在下一个FixedUpdate中执行（如果角色在地面上）。
        /// </summary>
        /// <param name="jumpVelocity">起跳速度 (x: 水平速度, y: 垂直速度)。行为层应根据前/后/原地跳传入不同的x值。</param>
        public void RequestJump(Vector2 jumpVelocity)
        {
            // Debug.Log($"[Movement] Jump Requested. Velocity={jumpVelocity}, Grounded={isGrounded}");
            if (_isGrounded)
            {
                _jumpRequested = true;
                _requestedJumpVelocity = jumpVelocity; // 存储请求的起跳速度
            }
        }

        /// <summary>
        /// 对角色施加一个力（冲量或持续力）。
        /// 用于处理击退、击飞、吸引等被动位移。
        /// 力应该主要作用于X和Y轴。
        /// </summary>
        /// <param name="forceXY">施加的力向量 (X, Y分量有效)。</param>
        /// <param name="mode">力的模式 (Impulse通常用于瞬间击打)。</param>
        public void ApplyForce(Vector2 forceXY, ForceMode2D mode = ForceMode2D.Impulse)
        {
            var force = new Vector2(forceXY.x, forceXY.y); // 确保Z轴力为0
            // Debug.Log($"[Movement] ApplyForce: {force}, Mode: {mode}");
            // 考虑：是否要在施加力之前清除部分或全部现有速度？
            // 例如，被击飞时可能需要重置之前的移动速度
            _rb.velocity = Vector2.zero; // 如果需要完全覆盖之前的速度

            _rb.AddForce(force, mode);
        }

        /// <summary>
        /// 设置角色期望的视觉朝向。
        /// 由行为层调用，告知表现层应该朝向哪边。
        /// </summary>
        /// <param name="isFacingRight">true 表示朝右, false 表示朝左。</param>
        public void SetFacingDirection(bool isFacingRight)
        {
            // 简单验证输入
            _targetFacingDirection = isFacingRight ? 1 : -1;
            // 注意：实际的视觉更新发生在LateUpdate中
        }
        #endregion

        #region 内部逻辑

        /// <summary>
        /// 更新角色的地面状态。
        /// </summary>
        private void UpdateGroundedStatus()
        {
            if (groundCheckPoint is null) return;

            // 地面检测
            var hit = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
            _isGrounded = hit is not null;
            // Optional: 如果需要更精确的检测（例如斜坡），可以使用Physics.Raycast或Physics.SphereCast向下发射射线
        }

        /// <summary>
        /// 在FixedUpdate中应用主动移动。
        /// 使用Rigidbody.MovePosition来移动，以尊重物理碰撞。
        /// </summary>
        private void ApplyActiveMovement()
        {
            // 只处理X轴的主动移动
            if (Mathf.Abs(_currentMovementInput.x) > 0.01f)
            {
                // 计算本FixedUpdate帧的目标移动位置
                var horizontalDisplacement = new Vector2(_currentMovementInput.x, 0) * Time.fixedDeltaTime;
                var nextPosition = _rb.position + horizontalDisplacement;

                // 使用MovePosition移动，它会尝试移动到目标位置，并处理碰撞
                _rb.MovePosition(nextPosition);
                // Debug.Log($"[Movement] Applying Active Movement: Disp={horizontalDisplacement}, NextPos={nextPosition}");
            }

            // 注意：如果角色在空中，你可能希望禁用或减弱地面移动控制
            // if (!isGrounded) { /* 可能需要不同的空中移动逻辑 */ }
        }


        /// <summary>
        /// 在FixedUpdate中施加跳跃力（如果已请求且在地面）。
        /// </summary>
        private void ApplyJumpVelocity()
        {
            if (!_jumpRequested) return;
            // Debug.Log($"[Movement] Applying Jump Force. Grounded={isGrounded}");
            // 再次检查是否真的在地面（可能在 Update 到 FixedUpdate 之间状态变化）,并且确保只在地面时执行跳跃
            // 在下次 RequestJump 时会覆盖 _requestedJumpVelocity, 因此不需要重置
            if (_isGrounded)
            {
                // 直接设置速度，覆盖当前速度
                _rb.velocity = new Vector2(_requestedJumpVelocity.x, _requestedJumpVelocity.y);
                // 跳跃后立即标记为不在地面，避免连续跳跃（如果需要）
                _isGrounded = false;
            }
            // 无论是否成功跳跃，都消耗掉跳跃请求
            _jumpRequested = false;
        }

        /// <summary>
        /// 应用自定义重力，使跳跃感觉更可控。
        /// </summary>
        private void ApplyCustomGravity()
        {
            // 只有在空中时才施加额外的重力
            // if (!_isGrounded && _rb.velocity.y < 0) // 只在下落时增强重力，使上升阶段更平缓 (可选)
            if (!_isGrounded) // 或者只要在空中就增强重力
            {
                // Physics.gravity 是全局重力设置 (通常是 Vector3(0, -9.81f, 0))
                _rb.AddForce(Physics2D.gravity * ((gravityMultiplier - 1f) * _rb.mass)); // 施加额外的重力
                // 注意：这里乘以rb.mass是因为AddForce默认考虑质量，而Physics2D.gravity是加速度。
                // 或者直接修改速度：
                _rb.velocity += Physics2D.gravity * ((gravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
            // 如果不使用自定义重力，Rigidbody会自动应用场景设置的重力
        }

        private void ApplyVisualFacingDirection()
        {
            var currentScaleX = visualTransform.localScale.x;
            var targetScaleX = Mathf.Abs(currentScaleX) * _targetFacingDirection;

            if (!Mathf.Approximately(currentScaleX, targetScaleX))
            {
                visualTransform.localScale = 
                    new Vector3(targetScaleX, visualTransform.localScale.y, visualTransform.localScale.z);
            }
        }
        #endregion

        #region Gizmos (调试用)

        private void OnDrawGizmosSelected()
        {
            // 绘制地面检测范围
            if (groundCheckPoint is not null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }

        #endregion

        #region 公开属性 (供其他系统查询状态)

        /// <summary>
        /// 角色当前是否接触地面。
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// 获取角色当前的物理速度。
        /// </summary>
        public Vector3 CurrentVelocity => _rb.velocity;
        
        /// <summary>
        /// 获取当前应用的视觉朝向。
        /// 注意：这反映了视觉上的朝向，可能与行为层计算的目标朝向有瞬间差异。
        /// </summary>
        public bool FacingRight => _targetFacingDirection == 1;

        #endregion
    }
}