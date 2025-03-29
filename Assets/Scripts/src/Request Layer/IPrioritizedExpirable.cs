namespace src.Request_Layer
{
    public interface IPrioritizedExpirable
    {
        string Name { get; }
        int Priority { get; set; }
        int Lifetimes { get; set; }
    }
}