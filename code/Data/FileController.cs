using System.Text.Json;

namespace Sandbox.Data;
public class FileController
{
	public Settings set { get; set; }
	public List<MapData> maps { get; set; }
	string s = "Settings.json";
	string m = "Maps.json";
	public void ReadFiles()
	{
		if ( FileSystem.Data.FileExists( s ) )
		{
			set = FileSystem.Data.ReadJson<Settings>( s );
		}
		else
		{
			set = new Settings();
			FileSystem.Data.WriteAllText( s, ObjToJson( set ) );
		}
		if ( FileSystem.Data.FileExists( m ) )
		{
			maps = JsonSerializer.Deserialize<List<MapData>>( FileSystem.Data.ReadAllText( m ) );
		}
		else
		{
			maps = new List<MapData>();
			FileSystem.Data.WriteAllText( m, ObjToJson( maps ) );
		}
	}
	public void SaveMaps()
	{
		FileSystem.Data.WriteAllText( s, ObjToJson( maps ) );
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
		foreach ( var item in maps )
		{

		}
	}
}
