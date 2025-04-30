using System.Collections.Generic;
using System.Linq;
using src.Behavior_Layer;
using src.Request_Layer.Condition;
using UnityEngine;

namespace src.Request_Layer
{
    [CreateAssetMenu(menuName = "Character/Mapping Rule")]
    public class MappingRuleSO : ScriptableObject
    {
        [SerializeReference] 
        public List<ConditionBase> conditions;

        public List<BaseBehaviorConfigSO> requiredStateIDs;
        [Tooltip("映射的触发器名称")] public string mappingResult;            
        public int priority;

        public bool MatchesContext(ContextData context)
        {
            if (conditions is null || conditions.Count == 0)
                return true;
            return conditions.All(condition => condition.Evaluate(context));
        }
        
        // #if UNITY_EDITOR
        // private void OnValidate()
        // {
        //     throw new NotImplementedException();
        // }
        // #endif
    }
}