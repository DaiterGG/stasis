using Stasis.Data;
using Sandbox.Services;
using System.Threading.Tasks;

namespace Stasis;

public class LBControl
{
	static async Task<Leaderboards.Board2> GetMapLB( string mapInd, int amount, bool centerOnMe = false, bool yourCountry = false )
	{
		var ind = Game.Ident;
		var lb = Leaderboards.GetFromStat( ind, mapInd );
		if ( yourCountry )
			lb.SetCountryCode( "auto" );
		if ( centerOnMe )
			lb.CenterOnMe();
		lb.MaxEntries = amount;
		lb.SetAggregationMin();
		await lb.Refresh();
		return lb;
	}

	public static async Task<List<Score>> GetScores( string mapInd, int amount, bool centerOnMe = false, bool yourCountry = false )
	{
		var sc = new List<Score>();
		Sng.ELog( mapInd );
		if ( Game.IsEditor && Sng.TestLeaderboards ) mapInd += "test";
		Sng.ELog( mapInd );
		var board = await GetMapLB( mapInd, amount, centerOnMe, yourCountry );
		foreach ( var e in board.Entries )
		{
			Replay? Replay = e.Data == null ? null : ReplaySerialize.DictToReplay( e.Data );
			sc.Add( new Score( (float)e.Value, e.Timestamp.DateTime, e.DisplayName, e.SteamId, Replay, mapInd ) );
		}
		return sc;
	}

	public static void SetScore( Score scr, string mapInd )
	{
		if ( Game.IsEditor && Sng.TestLeaderboards ) mapInd += "test";
		if ( !ValidateScore( scr ) ) return;
		if ( scr.Replay is Replay r )
		{
			Stats.SetValue( mapInd, scr.Time, "Replay", ReplaySerialize.RemoveTicksArray( r ) );
		}
		else Stats.SetValue( mapInd, scr.Time );
		Log.Info( "Stat is successfully set: " + " " + mapInd + " " + scr.Time );
	}
	public static bool ValidateScore( Score scr )
	{
		if ( scr.Time == 0f ) return false;
		return true;
	}
}
