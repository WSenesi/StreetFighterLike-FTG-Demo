using System.Collections.Generic;
using Sirenix.OdinInspector;
using src.Behavior_Layer.EventConfig;
using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(fileName = "Move Behavior", menuName = "MyScriptableObject/Character/Move Behavior")]
    public class MoveBehaviorSO : ScriptableObject
    {
        public string behaviorName;
        public int duration;
        public StateType behaviorType;
        public bool needsExitTime;
        
        // TODO: Define ConfigData and Event
        [SerializeReference]
        public List<EventConfigBase> events = new List<EventConfigBase>();


        [Button("Sort Events by startFrame")]
        private void SortEvents()
        {
            events.Sort((a, b) => a.startFrame.CompareTo(b.startFrame));
        }
    }

    
    
}