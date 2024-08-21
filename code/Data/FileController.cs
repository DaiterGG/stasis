using System;
using System.Text.Json;

namespace Sandbox.Data;
public class FileController
{
	public Settings set { get; set; }
	public List<MapData> maps { get; set; }
	public MapData currentMap;
	public float currentTime = 0;
	string s = "Settings.json";
	string m = "Maps.json";
	static Sng SNG = Sng.Inst;
	public void ReadFiles()
	{
		if ( FileSystem.Data.FileExists( s ) )
		{
			try
			{
				set = FileSystem.Data.ReadJson<Settings>( s );

			}
			catch ( Exception err )
			{
				Log.Warning( err.Message );
			}
		}
		else
		{
			set = new Settings();
			FileSystem.Data.WriteAllText( s, ObjToJson( set ) );
		}
		if ( FileSystem.Data.FileExists( m ) )
		{
			try
			{
				maps = JsonSerializer.Deserialize<List<MapData>>( FileSystem.Data.ReadAllText( m ) );
			}
			catch ( Exception err )
			{
				Log.Warning( err.Message );
			}
		}
		else
		{
			maps = new List<MapData>();
			FileSystem.Data.WriteAllText( m, ObjToJson( maps ) );
		}
	}
	public void SaveMaps()
	{
		FileSystem.Data.WriteAllText( m, ObjToJson( maps ) );
	}
	public void SaveSettings()
	{
		FileSystem.Data.WriteAllText( s, ObjToJson( set ) );
	}
	private string ObjToJson( object o )
	{
		return JsonSerializer.Serialize( o, new JsonSerializerOptions
		{
			WriteIndented = true
		} );
	}
	public void MapInfoSerialize( Info info )
	{
		if ( info == null )
		{
			currentMap = null;
			return;
		}
		currentMap = maps.FirstOrDefault( map =>
		{
			return map.Indent == SNG.MapIndent &&
			map.Name == info.DisplayName &&
			map.Vesrion == info.Version;
		} );
		bool _ = false;
		if ( currentMap == default( MapData ) )
		{
			_ = true;
			currentMap = new MapData();
		}
		currentMap.Name = info.DisplayName;
		currentMap.Indent = SNG.MapIndent;
		currentMap.Vesrion = info.Version;
		foreach ( var item in info.Author )
		{
			currentMap.Authors.Add( item.Key );
			currentMap.AuthorLinks.Add( item.Value );
		}
		currentMap.DifficultyTier = info.DifficultyTier;
		currentMap.Description = info.Description;
		currentMap.SpeedRun = info.SpeerunMap;
		currentMap.GoldTime = info.SpeerunMap ? info.GoldTime : 0;
		currentMap.SilverTime = info.SpeerunMap ? info.SilverTime : 0;
		currentMap.BronzeTime = info.SpeerunMap ? info.BronzeTime : 0;

		if ( _ ) maps.Add( currentMap );

		SaveMaps();
	}
	public void SetScore( float time )
	{
		currentTime = time;
		if ( currentMap == null ) return;
		var scr = new Score( time, DateTime.Now );

		currentMap.Scores.Add( scr );
		currentMap.Scores.Sort( ( x, y ) => y.Time.CompareTo( x.Time ) );
		SaveMaps();
	}
}
