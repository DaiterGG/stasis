

namespace Stasis.Data;


public class Replay
{
	public List<Tick> Ticks { get; set; } = new List<Tick>();
	public Dictionary<int,List<Action>> Actions { get; set; } = new Dictionary<int,List<Action>>();
}

