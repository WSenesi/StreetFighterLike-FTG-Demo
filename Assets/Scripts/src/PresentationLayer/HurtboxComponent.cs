using UnityEngine;

namespace src.PresentationLayer
{
    /// <summary>
    /// 挂载在 Hurtbox 子物体下。
    /// 需要物体具有 Collider2D 组件
    /// 用于携带归属信息和控制接口
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class HurtboxComponent : MonoBehaviour
    {
        public string identifier;
        public Character.Character OwnerCharacter { get; private set; }
        
        private BoxCollider2D _collider2D;

        private void Awake()
        {
            _collider2D = GetComponent<BoxCollider2D>();
            OwnerCharacter = GetComponent<Character.Character>();
        }

        /// <summary>
        /// 根据事件配置，激活/禁用 Hurtbox 并设置其属性
        /// </summary>
        /// <param name="owner">Hurtbox 的归属角色</param>
        /// <param name="offset">事件指定的碰撞体偏移</param>
        /// <param name="size">事件指定的碰撞体大小</param>
        /// <param name="shouldBeActive">是否激活 Hurtbox</param>
        public void Configure(Character.Character owner, Vector2 offset, Vector2 size, bool shouldBeActive = true)
        {
            // OwnerCharacter = owner;
            _collider2D.enabled = shouldBeActive;

            if (shouldBeActive && _collider2D is not null)
            {
                _collider2D.offset = offset;
                _collider2D.size = size;
                _collider2D.isTrigger = true;
            }
        }
        
        /// <summary>
        /// 仅激活/禁用 Hurtbox, 使用其当前预设的偏移和大小
        /// </summary>
        /// <param name="owner">Hurtbox 的归属角色</param>
        /// <param name="shouldBeActive">是否激活 Hurtbox</param>
        public void SetActive(Character.Character owner, bool shouldBeActive)
        {
            // OwnerCharacter = owner;
            _collider2D.enabled = shouldBeActive;
        }

        public void SetOwnerCharacter(Character.Character owner)
        {
            OwnerCharacter = owner;
        }
    }
}
