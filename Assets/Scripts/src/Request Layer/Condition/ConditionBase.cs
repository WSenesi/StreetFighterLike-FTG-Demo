using System;
using src.Behavior_Layer;

namespace src.Request_Layer.Condition
{
    [Serializable]
    public abstract class ConditionBase
    {
        public abstract bool Evaluate(ContextData context);
        public virtual string GetDescription() => GetType().Name;
    }

    public enum Comparison
    {
        Equal,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
    }
}

