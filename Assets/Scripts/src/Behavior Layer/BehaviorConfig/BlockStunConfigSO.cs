using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(fileName = "Block Stun Config", menuName = "Character/Behavior/Block Stun")]
    public class BlockStunConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.BlockStun;
        
        public AnimationClip blockStunAnimation;
    }
}