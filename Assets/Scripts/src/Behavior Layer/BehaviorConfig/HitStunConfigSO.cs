using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(fileName = "Hit Stun Config", menuName = "Character/Behavior/Hit Stun")]
    public class HitStunConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.HitStun;
        
        [Header("Animation")]
        public AnimationClip hitStunAnimation;
    }
}