using Stasis.Data;
using Sandbox;
using Sandbox.Services;
using System.Threading.Tasks;
using Sandbox.Utility;

namespace Stasis;

public class LBControl
{
	public static readonly string ind = "move.stasis";

	static async Task<Leaderboards.Board2> GetMapLB(string mapInd, int amount,bool centerOnMe = false, bool yourCountry = false){
		var lb = Leaderboards.GetFromStat(ind, mapInd );
		if(yourCountry)
			lb.SetCountryCode("auto");
		if(centerOnMe)
			lb.CenterOnMe();
		lb.MaxEntries = amount;
		lb.SetAggregationMin();
		await lb.Refresh();
		return lb;
	}

	public static async Task<List<Score>> GetScores(string mapInd, int amount,bool centerOnMe = false, bool yourCountry = false)
	{
		var sc = new List<Score>();
		var board = await GetMapLB( mapInd, amount, centerOnMe, yourCountry );
		foreach( var e in board.Entries){
			sc.Add( new Score((float)e.Value, e.Timestamp.DateTime, e.DisplayName, e.SteamId) );
		}
		return sc;
	}

	public static void SetScore(Score scr, string mapInd){
		if (!ValidateScore(scr)) return;
		Stats.SetValue(mapInd, scr.Time);
		Log.Info("Stat is successfully set: " + scr.Time);
	}
	public static bool ValidateScore(Score scr){
		if( scr.Time == 0f) return false;
		return true;
	}
}
