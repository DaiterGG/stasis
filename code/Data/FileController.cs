using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sandbox.Data;
public class FileController
{
	public Settings Set { get; set; }
	public List<MapData> Maps { get; set; } = new List<MapData>();
	public MapData currentMap;
	public float currentTime = 0;
	string s = "Settings.json";
	string m = "Maps.json";
	static Sng SNG = Sng.Inst;


	static public string[] FeaturedMaps = new string[]
	{

	};
	static public string[] OfficialMaps = new string[]
	{
		"move.stasis_playground_scene",
	};

	public void ReadFiles()
	{
		if ( FileSystem.Data.FileExists( s ) )
		{
			try
			{
				Set = FileSystem.Data.ReadJson<Settings>( s );

			}
			catch ( Exception err )
			{
				Log.Warning( err.Message );
			}
		}
		else
		{
			Set = new Settings();
			FileSystem.Data.WriteAllText( s, ObjToJson( Set ) );
		}
		if ( FileSystem.Data.FileExists( m ) )
		{
			try
			{
				Maps = JsonSerializer.Deserialize<List<MapData>>( FileSystem.Data.ReadAllText( m ) );
			}
			catch ( Exception err )
			{
				Log.Warning( err.Message );
			}
		}
		else
		{
			Maps = new List<MapData>();
			FileSystem.Data.WriteAllText( m, ObjToJson( Maps ) );
		}
		AddOfficialMaps();
	}
	public void AddOfficialMaps()
	{
		foreach ( var i in FeaturedMaps )
		{
			FetchNewMap( i, "featured" );
		}
		foreach ( var i in OfficialMaps )
		{
			FetchNewMap( i, "official" );
		}
	}
	public void SaveMaps()
	{
		FileSystem.Data.WriteAllText( m, ObjToJson( Maps ) );
	}
	public void SaveSettings()
	{
		FileSystem.Data.WriteAllText( s, ObjToJson( Set ) );
	}
	private string ObjToJson( object o )
	{
		return JsonSerializer.Serialize( o, new JsonSerializerOptions
		{
			WriteIndented = true
		} );
	}
	public async void FetchNewMap( string indent, string type )
	{

		var found = Maps.FirstOrDefault( x => x.Indent == indent );
		if ( found == default( MapData ) )
		{
			var mp = new MapData();
			mp.Type = type;
			Maps.Add( mp );
			await FetchMap( indent, mp );
		}
		SaveMaps();
	}
	static public async Task FetchMap( string packageIndent, MapData mapData )
	{
		var package = await Package.Fetch( packageIndent, false );
		mapData.Name = package.Title;
		mapData.Description = package.Description;
		mapData.Indent = package.FullIdent;
		mapData.Img = package.Thumb;
	}
	static public async void DownloadMap( string packageIndent )
	{
		var package = await Package.Fetch( packageIndent, false );
		var meta = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		await package.MountAsync();
		Sng.Inst.LoadNewMap( meta, false );
	}
	public void SetCurrentMap( string ind )
	{
		currentMap = Maps.FirstOrDefault( map =>
		{
			return map.Indent == ind;
		} );
		if ( currentMap == default( MapData ) )
		{
			Log.Warning( "MAP LOADING WAS NOT FECHED CORRECLTY" );
		}
	}
	//TODO FETCH 2 WAYS
	public void InfoSerialize( Info info )
	{
		if ( info == null )
		{
			currentMap = null;
			return;
		}
		try
		{

			currentMap.Vesrion = info.Version;
			var auth = new List<string>();
			var authl = new List<string>();
			foreach ( var item in info.Author )
			{
				auth.Add( item.Key );
				authl.Add( item.Value );
			}
			currentMap.Authors = auth;
			currentMap.AuthorLinks = authl;

			currentMap.DifficultyTier = info.DifficultyTier;
			currentMap.SpeedRun = info.SpeerunMap;
			currentMap.GoldTime = info.SpeerunMap ? info.GoldTime : 0;
			currentMap.SilverTime = info.SpeerunMap ? info.SilverTime : 0;
			currentMap.BronzeTime = info.SpeerunMap ? info.BronzeTime : 0;

		}
		catch ( Exception err ) { Log.Warning( "Map serialize error: " + err.Message ); }

		SaveMaps();
	}
	public void SetScore( float time )
	{
		currentTime = time;
		if ( currentMap == null ) return;
		var scr = new Score( time, DateTime.Now );

		currentMap.Scores.Add( scr );
		currentMap.Scores.Sort( ( x, y ) => x.Time.CompareTo( y.Time ) );
		SaveMaps();
	}
}
