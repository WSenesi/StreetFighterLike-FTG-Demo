using src.Input_Layer;

namespace src.UI
{
    public class InputRecordData
    {
        public readonly Direction Direction;
        public readonly Attack Attack;
        // private int _duration;
        public int Duration;
        
        public InputRecordData(Direction direction, Attack attack, int duration)
        {
            this.Direction = direction;
            this.Attack = attack;
            this.Duration = duration;
        }
    }
}