namespace Sandbox.Data;
public class MapData
{
	public MapData()
	{
		Scores = new List<Score>();
	}
	public List<Score> Scores { get; set; }
	public string Name { get; set; }
	public string Img { get; set; }
	public string Author { get; set; }
	public string Indent { get; set; }
	public string Meta { get; set; }
	public string Description { get; set; }
	public string Version { get; set; }
	public string Type { get; set; }
	public int DifficultyTier { get; set; }
	public bool SpeedRun { get; set; }
	public float GoldTime { get; set; }
	public float SilverTime { get; set; }
	public float BronzeTime { get; set; }

}
