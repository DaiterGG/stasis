namespace Sandbox.Data;
public class MapData
{
	public MapData()
	{
		Scores = new List<Score>();
		Authors = new List<string>();
		AuthorLinks = new List<string>();
	}
	public List<Score> Scores { get; set; }
	public string Name { get; set; }
	public string Img { get; set; }
	public List<string> Authors { get; set; }
	public List<string> AuthorLinks { get; set; }
	public string Indent { get; set; }
	public string Description { get; set; }
	public string Vesrion { get; set; }
	public string Type { get; set; }
	public int DifficultyTier { get; set; }
	public bool SpeedRun { get; set; }
	public float GoldTime { get; set; }
	public float SilverTime { get; set; }
	public float BronzeTime { get; set; }

}
