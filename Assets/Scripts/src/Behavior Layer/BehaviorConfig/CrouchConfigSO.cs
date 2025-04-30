using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(menuName = "Character/Behavior/Crouch")]
    public class CrouchConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.Crouch;
        
        public float transitionDuration = 0.5f;
        
        [Header("蹲下")]
        public AnimationClip crouchDownAnimation;
        public AudioClip crouchDownSound;
        
        [Header("站起")]
        public AnimationClip standUpAnimation;
        public AudioClip standUpSound;
    }
}