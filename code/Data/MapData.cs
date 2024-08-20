namespace Sandbox.Data;
public class MapData
{
	public MapData()
	{
		Scores = new List<Score>();
	}
	public List<Score> Scores { get; set; }
	public string Name { get; set; }
	public List<string> Authors { get; set; }
	public List<string> AuthorLinks { get; set; }
	public string Indent { get; set; }
	public string Description { get; set; }
	public string Vesrion { get; set; }
}
