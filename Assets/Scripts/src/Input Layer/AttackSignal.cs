using System;
using Sirenix.OdinInspector;

namespace src.Input_Layer
{
    [Flags]
    public enum Attack
    {
        None = 0,
        LightPunch = 1,
        LightKick = 2,
        MediumPunch = 4,
        MediumKick = 8,
        HeavyPunch = 16,
        HeavyKick = 32,
    }
    [Serializable]
    public class AttackSignal : ISignal
    {
        [HorizontalGroup("AttackSignal")]
        [VerticalGroup("AttackSignal/Attack")]
        public Attack attack = Attack.None;
        [VerticalGroup("AttackSignal/Duration")]
        public int duration = 1;
    
        public int Duration => duration;
    
        public AttackSignal(Attack attack, int duration)
        {
            this.attack = attack;
            this.duration = duration;
        }
        /// <summary>
        /// flag为非0数, 判断对应位是否为1; flag为0, 判断attack是否为全零
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool Contains(Attack flag)
        {
            if (flag == Attack.None)
                return (attack & ~flag) == flag;
            return (attack & flag) == flag;
        }
    }
}