using Stasis.Data;
using Sandbox;
using Sandbox.Services;
using System.Threading.Tasks;
using Sandbox.Utility;

namespace Stasis;

public class LBControl
{
    public static readonly string ind = "move.stasis";

    static async Task<Leaderboards.Board2> GetMapLB(string mapInd, int amount, bool centerOnMe = false, bool yourCountry = false)
    {
        var lb = Leaderboards.GetFromStat(ind, mapInd);
        if (yourCountry)
            lb.SetCountryCode("auto");
        if (centerOnMe)
            lb.CenterOnMe();
        lb.MaxEntries = amount;
        lb.SetAggregationMin();
        await lb.Refresh();
        return lb;
    }

    public static async Task<List<Score>> GetScores(string mapInd, int amount, bool centerOnMe = false, bool yourCountry = false)
    {
        var sc = new List<Score>();
        if (Game.IsEditor) mapInd += "test";
        var board = await GetMapLB(mapInd, amount, centerOnMe, yourCountry);
        foreach (var e in board.Entries)
        {
            sc.Add(new Score()
            {
                Time = (float)e.Value,
                Date = e.Timestamp.DateTime,
                DisplayName = e.DisplayName,
                SteamID = e.SteamId,
                Replay = e.Data == null ? null : ReplaySerialize.JsonToReplay(e.Data)
            });
        }
        return sc;
    }

    public static void SetScore(Score scr, string mapInd)
    {
        if (Game.IsEditor) mapInd += "test";
        if (!ValidateScore(scr)) return;
        if (scr.Replay is Replay r)
        {
            Stats.SetValue(mapInd, scr.Time, "Replay", ReplaySerialize.RemoveTicksArray(r));
        }
        else Stats.SetValue(mapInd, scr.Time);
        Log.Info("Stat is successfully set: " + " " + mapInd + " " + scr.Time);
    }
    public static bool ValidateScore(Score scr)
    {
        if (scr.Time == 0f) return false;
        return true;
    }
}
