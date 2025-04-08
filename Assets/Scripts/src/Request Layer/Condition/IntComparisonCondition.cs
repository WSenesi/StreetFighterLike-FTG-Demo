using System;
using src.Behavior_Layer;

namespace src.Request_Layer.Condition
{
    [Serializable]
    public abstract class IntComparisonCondition : ConditionBase
    {
        public Comparison comparisonType;
        public int threshold;
        
        protected abstract int GetValue(ContextData context);

        public override bool Evaluate(ContextData context)
        {
            int value = GetValue(context);
            return comparisonType switch
            {
                Comparison.Equal => value == threshold,
                Comparison.GreaterThan => value > threshold,
                Comparison.GreaterThanOrEqual => value >= threshold,
                Comparison.LessThan => value < threshold,
                Comparison.LessThanOrEqual => value <= threshold,
                _ => false
            };
        }
    }
}