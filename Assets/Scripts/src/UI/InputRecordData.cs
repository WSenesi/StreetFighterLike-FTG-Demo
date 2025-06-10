using src.Input_Layer;

namespace src.UI
{
    public class InputRecordData
    {
        public Direction Direction;
        public Attack Attack;
        public int Duration;
        
        public InputRecordData(Direction direction, Attack attack, int duration)
        {
            this.Direction = direction;
            this.Attack = attack;
            this.Duration = duration;
        }
    }
}