using UnityEngine;

namespace src.Behavior_Layer
{
    public enum JumpDirection
    {
        Neutral,
        Forward,
        Backward,
    }
    
    [CreateAssetMenu(fileName = "Jump Config", menuName = "Character/Behavior/Jump")]
    public class JumpConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.Jump;
        
        public JumpDirection jumpDirection;
        public float verticalVelocity;
        public float horizontalVelocity;
        public float gravity;
        public int jumpDuration;
        
        public AnimationClip jumpAnimation;
        public AnimationClip landingAnimation;
        public AudioClip landingSound;
    }
}