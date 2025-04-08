using System;
using src.Behavior_Layer;

namespace src.Request_Layer.Condition
{
    [Serializable]
    public abstract class FloatComparisonCondition : ConditionBase
    {
        public Comparison comparisonType;
        public float threshold;

        protected abstract float GetValue(ContextData context);
        
        public override bool Evaluate(ContextData context)
        {
            float value = GetValue(context);
            return comparisonType switch
            {
                Comparison.Equal => Math.Abs(value - threshold) < 0.0001f,
                Comparison.GreaterThan => value > threshold,
                Comparison.GreaterThanOrEqual => value >= threshold,
                Comparison.LessThan => value < threshold,
                Comparison.LessThanOrEqual => value <= threshold,
                _ => false
            };
        }
    }
}