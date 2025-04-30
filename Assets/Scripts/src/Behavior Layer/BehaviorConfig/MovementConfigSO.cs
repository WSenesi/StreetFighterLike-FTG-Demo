using UnityEngine;

namespace src.Behavior_Layer
{
    public enum WalkDirection
    {
        Forward, 
        Backward,
    }
    
    [CreateAssetMenu(fileName = "Move Config", menuName = "Character/Behavior/Move")]
    public class MovementConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.Walk;

        public WalkDirection walkDirection;
        public float speed = 4.7f;

        // Optional:
        public AnimationClip loopAnimation;
        public AudioClip loopSound;
    }
}