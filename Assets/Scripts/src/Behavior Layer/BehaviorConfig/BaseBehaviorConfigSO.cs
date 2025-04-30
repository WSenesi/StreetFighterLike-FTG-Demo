using UnityEngine;

namespace src.Behavior_Layer
{
    public abstract class BaseBehaviorConfigSO : ScriptableObject
    {
        public abstract StateType BehaviorType { get; }
        
        public string behaviorName;
        // public int duration;
        
        public bool needsExitTime;
        
        // [Header("Common Effects")]
        // public AnimationClip entryAnimation;
        // public AudioClip entrySound;
        
        // TODO: Hurtbox Profile
        
    }

    
    
}