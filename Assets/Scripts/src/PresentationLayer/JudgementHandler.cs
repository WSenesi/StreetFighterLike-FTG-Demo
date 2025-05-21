using System.Collections.Generic;
using src.Behavior_Layer;
using src.Behavior_Layer.EventConfig;
using UnityEngine;

namespace src.PresentationLayer
{
    public struct DetectedCollisionInfo
    {
        public HitboxEventConfig SrcHitboxConfig;
        public Character Attacker;
        public HurtboxComponent TargetHurtbox;
        public Character TargetCharacter;
    }
    
    public class JudgementHandler : MonoBehaviour
    {
        private Dictionary<string, HurtboxComponent> _hurtboxComponents = new();
        private Character _character;
        private CharacterEventManager _eventManager;

        private class ActiveHitboxData
        {
            public HitboxEventConfig Config { get; }
            public Character Attacker { get; }
            
            // 用于记录此 Hitbox 在此次激活期间击中过的目标, 防止单个 Hitbox 多帧持续命中同一目标时重复发送攻击事件
            public readonly HashSet<int> HitTargetsThisActivation = new HashSet<int>();

            public ActiveHitboxData(HitboxEventConfig config, Character attacker)
            {
                Config = config;
                Attacker = attacker;
            }
        }

        private readonly List<ActiveHitboxData> _activeHitboxes = new List<ActiveHitboxData>();

        private void Start()
        {
            _character = GetComponent<Character>();
            if (_character is null)
            {
                Debug.LogError("Character needs to be attached to a Character");
                enabled = false;
                return;
            }

            if (_character.EventManager is null)
            {
                Debug.LogError("Character event manager needs to be attached to a Character");
                enabled = false;
                return;
            }
            
            _eventManager = _character.EventManager;
            SubscribeToEvents();
            InitializeHurtboxes();
        }

        private void FixedUpdate()
        {
            if (_eventManager is null || _activeHitboxes.Count == 0) return;

            List<DetectedCollisionInfo> thisFrameCollisionsToReport = new();

            for (var i = _activeHitboxes.Count - 1; i >= 0; i--)
            {
                var activeHitbox = _activeHitboxes[i];
                var hitboxConfig = activeHitbox.Config;
                var attacker = activeHitbox.Attacker;
                var attackerTransform = attacker.transform;
                var relativeOffset = hitboxConfig.offset;

                if (attackerTransform.localScale.x < 0)
                {
                    relativeOffset.x *= -1;
                }
                
                var rotation = Quaternion.Euler(0, 0, attackerTransform.eulerAngles.z);;
                var worldOffset = rotation * relativeOffset;
                Vector2 worldPosition = attackerTransform.position + worldOffset;
                // float worldAngle = attackerTransform.eulerAngles.z + hitboxConfig.angle;
                
                var hits = Physics2D.OverlapBoxAll(worldPosition, hitboxConfig.size,
                    attackerTransform.eulerAngles.z, hitboxConfig.targetLayer);

                if (hits.Length <= 0) continue;
                foreach (var hit in hits)
                {
                    var hurtboxComponent = hit.GetComponent<HurtboxComponent>();
                    if (hurtboxComponent?.OwnerCharacter is not null &&
                        hurtboxComponent.OwnerCharacter != attacker)
                    {
                        var targetInstanceID = hurtboxComponent.GetInstanceID();

                        if (activeHitbox.HitTargetsThisActivation.Contains(targetInstanceID)) 
                            continue;
                        
                        var collisionInfo = new DetectedCollisionInfo()
                        {
                            SrcHitboxConfig = hitboxConfig,
                            Attacker = attacker,
                            TargetHurtbox = hurtboxComponent,
                            TargetCharacter = hurtboxComponent.OwnerCharacter,
                        };
                        thisFrameCollisionsToReport.Add(collisionInfo);
                        // 将目标加入已命中列表,防止此 Hitbox 在此次激活期内重复命中同一目标
                        activeHitbox.HitTargetsThisActivation.Add(targetInstanceID);
                    }
                }
            }

            if (thisFrameCollisionsToReport.Count > 0)
            {
                _eventManager.DetectedRawCollision(thisFrameCollisionsToReport, _character);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeHurtboxes()
        {
            var hurtboxes = GetComponentsInChildren<HurtboxComponent>();
            foreach (var hb in hurtboxes)
            {
                if (hb.identifier == "")
                {
                    Debug.LogWarning($"Hurtbox {hb.name} on {_character.name} has no identifier.");
                    continue;
                }

                if (_hurtboxComponents.TryGetValue(hb.identifier, out var component))
                {
                    Debug.LogError($"Duplicate Hurtbox Identifier '{hb.identifier}' found on Character {_character.name}. " +
                                   $"Original: {component.name}, New: {hb.name}.");
                }
                else
                {
                    _hurtboxComponents.Add(hb.identifier, hb);
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_eventManager is null) return;

            _eventManager.OnHitboxActivateRequest += HandleHitboxActivateRequest;
            _eventManager.OnHitboxDeactivateRequest += HandleHitboxDeactivateRequest;
            _eventManager.OnHurtboxActivateRequest += HandleHurtboxActivateRequest;
            _eventManager.OnHurtboxDeactivateRequest += HandleHurtboxDeactivateRequest;
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventManager is null) return;
            
            _eventManager.OnHitboxActivateRequest -= HandleHitboxActivateRequest;
            _eventManager.OnHitboxDeactivateRequest -= HandleHitboxDeactivateRequest;
            _eventManager.OnHurtboxActivateRequest -= HandleHurtboxActivateRequest;
            _eventManager.OnHurtboxDeactivateRequest -= HandleHurtboxDeactivateRequest;
        }
        
        // --- Hitbox 事件处理 ---
        private void HandleHitboxActivateRequest(HitboxEventConfig config,Character attacker)
        {
            if (attacker != _character) return;
            
            // 检查是否存在相同的 Hitbox 配置实例
            if (_activeHitboxes.Exists(hitbox => hitbox.Config.eventId == config.eventId))
            {
                Debug.LogWarning($"Hitbox with id {config.eventId} already exists");
                return;
            }
            
            _activeHitboxes.Add(new ActiveHitboxData(config, attacker));
        }

        private void HandleHitboxDeactivateRequest(HitboxEventConfig config,Character attacker)
        {
            if (attacker != _character) return;
            
            // 查找 Hitbox
            var existHitbox = _activeHitboxes.Find(hitbox => hitbox.Config.eventId == config.eventId);
            if (existHitbox != null)
            {
                _activeHitboxes.Remove(existHitbox);
                Debug.Log($"Hitbox Deactivated: {config.eventId} for {attacker.name}");
            }
        }
        
        // --- Hurtbox 事件处理 ---
        private HurtboxComponent GetHurtboxComponent(string identifier)
        {
            if (_hurtboxComponents.TryGetValue(identifier, out var hurtbox))
            {
                return hurtbox;
            }
            Debug.LogWarning($"Hurtbox with identifier '{identifier}' not found on Character {_character.name}.");
            return null;
        }
        
        private void HandleHurtboxActivateRequest(HurtboxEventConfig config, Character owner)
        {
            if (owner != _character) return;
            
            var hurtboxComponent = GetHurtboxComponent(config.identifier);
            if (hurtboxComponent is null)
            {
                Debug.LogWarning($"Hurtbox component needs to be attached to a Character");
                return;
            }
            
            hurtboxComponent.Configure(
                owner: owner, 
                offset: config.colliderOffset, 
                size: config.colliderSize, 
                shouldBeActive: config.isActive);
        }

        private void HandleHurtboxDeactivateRequest(HurtboxEventConfig config, Character owner)
        {
            if (owner != _character) return;
            
            var hurtboxComponent = GetHurtboxComponent(config.identifier);
            hurtboxComponent?.SetActive(owner, !config.isActive);
            // TODO: 暂时无法处理(头身脚等)常驻的 Hurtbox 仅调整偏移和大小的逻辑
        }


        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _activeHitboxes is null || _activeHitboxes.Count == 0 || _character is null) 
                return;

            foreach (var activeHitbox in _activeHitboxes)
            {
                var hitboxConfig = activeHitbox.Config;
                var attacker = activeHitbox.Attacker;
                if (attacker is null) continue;
                
                var attackerTransform = attacker.transform;
                var relativeOffset = hitboxConfig.offset;
                if (attackerTransform.localScale.x < 0)
                    relativeOffset.x *= -1;
                
                Quaternion rotation = Quaternion.Euler(0, 0, attackerTransform.localEulerAngles.z);
                var worldOffset = rotation * relativeOffset;
                Vector2 worldPosition = attackerTransform.position + worldOffset;

                Gizmos.color = Color.red;
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(worldPosition, Quaternion.Euler(0, 0, attackerTransform.eulerAngles.z),
                    new Vector3(hitboxConfig.size.x, hitboxConfig.size.y, 0.1f));
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = oldMatrix;
            }
        }

        #endregion
    }
}