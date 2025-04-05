using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 输入层，将输入转化为信号，存储在环形缓冲区中
/// </summary>
[Required("Character")]
public class InputLayer : MonoBehaviour
{
    public Transform player;
    public Transform opponent;
    
    public InputBuffer<DirectionSignal> directionInput;
    public InputBuffer<AttackSignal> attackInput;

    [SerializeField] private bool _isInLeft = true;

    public InputLayer(Transform player, Transform opponent)
    {
        this.player = player;
        this.opponent = opponent;
    }

    public void Awake()
    {
        directionInput = new InputBuffer<DirectionSignal>();
        attackInput = new InputBuffer<AttackSignal>();
    }

    public void Update()
    {
        _isInLeft = IsPlayerInLeft();
        DirectionInput();
        AttackInput();
    }

    private void DirectionInput()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        Direction direction = Direction.Idle;
        DirectionSignal signal = directionInput.Read(0);
        // 由于每一帧声明了一个新的 InputSignal 变量，所以不需要将未输入的方向位 置0
        // 水平输入
        // 玩家在左侧
        if (_isInLeft)
        {
            switch (horizontal)
            {
                case > 0:
                    direction |= Direction.Front;               // 00XX |= 0001  ->  00X1
                    break;
                case < 0:
                    direction |= Direction.Back;                // 00XX |= 0010  ->  001X
                    break;
            }
        }
        // 玩家在右侧
        else
        {
            switch (horizontal)
            {
                case > 0:
                    direction |= Direction.Back;
                    break;
                case < 0:
                    direction |= Direction.Front;
                    break;
            }
        }

        switch (vertical)
        {
            // 竖直输入
            case > 0:
                direction |= Direction.Up;
                break;
            case < 0:
                direction |= Direction.Down;
                break;
            default:
                direction &= ~Direction.Up;
                break;
        }

        // 判断当前帧输入方向是否与上一帧相同，相同则只修改输入信号的持续时长；不同则录入新的信号
        if (signal != null && direction == signal.direction)
        {
            signal.duration = signal.duration >= 99 ? 99 : signal.duration + 1;
        }
        else
        {
            directionInput.Write( new DirectionSignal(direction, 1) );

            //if (signal != null)
            //    Debug.Log("输入方向信号：" + signal.direction + " 持续" + signal.duration + "帧");
        }
    }

    private void AttackInput()
    {
        bool lightPunch = Input.GetKey(KeyCode.U);
        bool lightKick = Input.GetKey(KeyCode.J);
        bool mediumPunch = Input.GetKey(KeyCode.I);
        bool mediumKick = Input.GetKey(KeyCode.K);
        bool heavyPunch = Input.GetKey(KeyCode.O);
        bool heavyKick = Input.GetKey(KeyCode.L);
        Attack attack = Attack.None;
        AttackSignal signal = attackInput.Read(0);

        // 轻拳
        if (lightPunch)
            attack |= Attack.LightPunch;
        else
            attack &= ~Attack.LightPunch;

        // 轻脚
        if (lightKick)
            attack |= Attack.LightKick;
        else
            attack &= ~Attack.LightKick;

        // 中拳
        if (mediumPunch)
            attack |= Attack.MediumPunch;
        else
            attack &= ~Attack.MediumPunch;

        // 中脚
        if (mediumKick)
            attack |= Attack.MediumKick;
        else
            attack &= ~Attack.MediumKick;

        // 重拳
        if (heavyPunch)
            attack |= Attack.HeavyPunch;
        else
            attack &= ~Attack.HeavyPunch;

        // 重脚
        if (heavyKick)
            attack |= Attack.HeavyKick;
        else
            attack &= ~Attack.HeavyKick;

        // 
        if (signal != null && attack == signal.attack)
        {
            signal.duration = signal.duration >= 99 ? 99 : signal.duration + 1;
        }
        else
        {
            attackInput.Write( new AttackSignal(attack, 1) );

            //if (signal != null)
            //    Debug.Log("输入攻击信号：" + signal.attack + " 持续" + signal.duration + "帧");
        }
    }

    private bool IsPlayerInLeft()
    {
        return player.position.x < opponent.position.x;
    }
}
