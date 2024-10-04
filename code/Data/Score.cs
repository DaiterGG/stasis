using System;

namespace Stasis.Data;
public class Score
{
    /*
    public Score( float Time, DateTime Date, string DisplayName, long SteamID, Replay Replay)
    {
        this.Time = Time;
        this.Date = Date;
        this.DisplayName = DisplayName;
        this.SteamID = SteamID;
        this.Replay = Replay;
    }
    */
    public float Time { get; set; }
    public DateTime Date { get; set; }
    public string DisplayName { get; set; }
    public long SteamID { get; set; }
    public Replay? Replay { get; set; }
}
