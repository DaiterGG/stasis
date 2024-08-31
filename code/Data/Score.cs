using System;

namespace Stasis.Data;
public class Score
{
	public Score( float time, DateTime date, string name = "Error Name", long steamid = 0 )
	{
		Time = time;
		Date = date;
		DisplayName = name;
		SteamID = steamid;
	}
	public float Time { get; set; }
	public DateTime Date { get; set; }
	public string DisplayName { get; set; }
	public long SteamID { get; set; }
}
