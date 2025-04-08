using System;
using src.Behavior_Layer;

namespace src.Request_Layer.Condition
{
    [Serializable]
    public class HasTagCondition : ConditionBase
    {
        public CharacterContextFlag requiredTag;
        
        public override bool Evaluate(ContextData context)
        {
            return context.ContainsFlag(requiredTag);
        }

        public override string GetDescription()
            => $"拥有标签: {requiredTag}";
    }
}