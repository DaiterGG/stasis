

namespace Stasis.Data;


public class Replay
{
    public Replay() { }
    public Replay(List<Tick> ticks, string ticksUTF, Dictionary<int, List<Action>> actions, int startTime, int endTime)
    {
        Ticks = ticks;
        TicksUTF = ticksUTF;
        Actions = actions;
        StartTime = startTime;
        EndTime = endTime;
    }
    //not a property so s&box serialize don't write it in file
    public List<Tick> Ticks = new List<Tick>();
    public string TicksUTF { get; set; }
    public Dictionary<int, List<Action>> Actions { get; set; } = new Dictionary<int, List<Action>>();
    public int StartTime { get; set; }
    public int EndTime { get; set; }
}

