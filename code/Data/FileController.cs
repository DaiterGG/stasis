using System;
using System.Text.Json;

namespace Sandbox.Data;
public class FileController
{
	public Settings set { get; set; }
	public List<MapData> maps { get; set; }
	public MapData currentMap;
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
		currentMap = maps.FirstOrDefault( map =>
		{
			return map.Indent == SNG.MapIndent &&
			map.Name == info.DisplayName &&
			map.Vesrion == info.Version;
		} );
		if ( currentMap == default( MapData ) )
		{
			currentMap = new MapData();
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

			maps.Add( currentMap );
		}
		else
		{
			Log.Info( currentMap.Name );
		}
		SaveMaps();
	}
	public void AddScore( Score score )
	{
		currentMap.Scores.Add( score );
		currentMap.Scores.Sort( ( x, y ) => y.Time.CompareTo( x.Time ) );
		SaveMaps();
	}
}
