using System;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox.Utility;
using Sandbox.Network;
namespace Stasis.Data;
public sealed class FileControl
{
	public Settings Set { get; set; }
	public List<MapData> Maps { get; set; } = new List<MapData>();
	public MapData currentMap;
	public float currentTime = 0;
	static string s = "Settings.json";
	static string m = "Maps.json";
	Sng SNG;
	public string[] FeaturedMaps;
	public string[] OfficialMaps;
	public void OnAwakeInit()
	{
		SNG = Sng.Inst;
		FeaturedMaps = new string[] { };
		OfficialMaps = new string[]
		{
		"move.plground",
		"move.hexagon2",
		"move.sharp2",
		};

		FilesInit();
	}

	public void FilesInit()
	{
		Set = new Settings();
		if ( FileSystem.Data.FileExists( s ) )
		{
			try
			{
				Set = FileSystem.Data.ReadJson<Settings>( s );
			}
			catch ( Exception err ) { Log.Error( err.Message ); }
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
				Maps = JsonSerializer.Deserialize<List<MapData>>( FileSystem.Data.ReadAllText( m ) , new JsonSerializerOptions { IncludeFields = true } );
			}
			catch ( Exception err )
			{
				Log.Warning( err.Message );
			}
		}
		else
		{
			try
			{
				Maps = new List<MapData>();
				FileSystem.Data.WriteAllText( m, ObjToJson( Maps ) );

			}
			catch ( Exception err ) { Log.Warning( "Move/Stasis folder does not exist?" + err.Message ); }
		}
		Log.Info(Maps.Count());
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
	public static void SaveMaps()
	{
		FileSystem.Data.WriteAllText( m, ObjToJson( Sng.Inst.FileC.Maps ) );

		Sng.Inst.MenuC.UpdateMapsList();
	}
	public static void SaveSettings()
	{
		FileSystem.Data.WriteAllText( s, ObjToJson( Sng.Inst.FileC.Set ) );
	}
	private static string ObjToJson( object o )
	{
		return JsonSerializer.Serialize( o, new JsonSerializerOptions
		{
			WriteIndented = true
		} );
	}
	public void FetchNewMap( string indent, string type )
	{
		var found = Maps.FirstOrDefault( x =>
		{

			Sng.ELog("indent1 = " + indent + " indent2 = " + x.Indent);
			return x.Indent == indent;
		} );
		if ( found != default( MapData ) && found != null && (found.Img == null || found.Name == null || found.Description == null) )
		{
			Log.Warning( "Bad data, fetch new one" );
			Log.Info( "removed: " + Maps.Remove( found ).ToString() );
			found = default;
		}
		var _new = false;
			Sng.ELog(found);
			Sng.ELog(found == default( MapData ));
		if ( found == default( MapData ) || found == null )
		{
			Sng.ELog("new");
			found = new MapData
			{
				Type = type
			};
			_new = true;
		}
		try
		{
			FetchMap( indent, found ).Wait();
		}
		catch ( Exception e ) { Log.Info( "Fetching map failed, are you offline? " + e.Message ); }

		if ( _new ) Maps.Add( found );


		SaveMaps();
	}
	static public async Task FetchMap( string packageIndent, MapData mapData )
	{
		var package = await Package.Fetch( packageIndent, true );
		if ( package == null ) throw new Exception( "Fetching failled" );
		mapData.Name = package.Title;
		mapData.Description = package.Summary;
		mapData.Indent = package.FullIdent;
		mapData.Img = package.Thumb;
	}
	public void DownloadAndLoad( string packageIndent )
	{
		SetCurrentMap( packageIndent );
		try
		{
			DownloadScene( packageIndent ).Wait();
		}
		catch ( Exception e ) { Log.Info( "Download failed, try again" + e ); }

		SNG.LoadNewMap( tempFile );
	}
	static SceneFile tempFile {get; set;} = new SceneFile();
	public async Task DownloadScene( string sceneIndent )
	{
		var package = await Package.Fetch( sceneIndent, false );

		var meta = package.GetMeta( "PrimaryAsset", "ERROR" );
		var g = await package.MountAsync();

		//tempFile = package.GetMeta<SceneFile>( "PrimaryAsset" );

		tempFile.LoadFromJson( g.ReadAllText( meta ) );
		//Sng.Inst.Scene.Load( scene );
	}
	public void SetCurrentMap( string ind )
	{
		currentMap = Maps.FirstOrDefault( map =>
		{
			return map.Indent == ind;
		} );
		if ( currentMap == default( MapData ) || currentMap == null )
		{
			Log.Warning( "Fetched data Don't have that map" );
			return;
		}

	}

	public void InfoSerialize( Info info )
	{
		if ( info == null )
		{
			currentMap = null;
			return;
		}
		if ( currentMap == null )
		{
			Log.Warning( "Map data was not fetched correctly" );
			return;
		}
		try
		{

			currentMap.Version = info.Version;
			currentMap.Author = info.Author;
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
		var scr = new Score( time, DateTime.Now, Steam.PersonaName, (long)Steam.SteamId );
		
		LBControl.SetScore(scr, currentMap.Indent);

		currentMap.Scores.Add( scr );
		currentMap.Scores.Sort( ( x, y ) => x.Time.CompareTo( y.Time ) );
		SaveMaps();
	}


}
