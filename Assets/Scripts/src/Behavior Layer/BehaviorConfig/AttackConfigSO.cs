using System.Collections.Generic;
using Sirenix.OdinInspector;
using src.Behavior_Layer.EventConfig;
using UnityEngine;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(fileName = "Attack Config", menuName = "Character/Behavior/Attack")]
    public class AttackConfigSO : BaseBehaviorConfigSO
    {
        public override StateType BehaviorType => StateType.Attack;

        public int duration;
        
        
        [SerializeReference]
        public List<EventConfigBase> events = new List<EventConfigBase>();
        
        
        [Button("Sort Events by startFrame")]
        private void SortEvents()
        {
            events.Sort((a, b) => a.startFrame.CompareTo(b.startFrame));
        }
    }
}