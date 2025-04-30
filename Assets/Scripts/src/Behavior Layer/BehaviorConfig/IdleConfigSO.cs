using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(menuName = "Character/Behavior/Idle")]
    public class IdleConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.Idle;

        public AnimationClip IdleAnimation;
    }
}