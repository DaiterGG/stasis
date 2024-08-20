using System.Text.Json;

namespace Sandbox.Data;
public class FileController
{
	public FileController()
	{
	}
	public void ReadFiles()
	{
		if ( FileSystem.Data.FileExists( "Settings.json" ) )
		{

		}
		else
		{
			set = new Settings();
			FileSystem.Data.WriteAllText( "Settings.json", ObjToJson( set ) );
		}
		if ( FileSystem.Data.FileExists( "Maps.json" ) )
		{

		}
		else
		{
			maps = new List<Map>();
			FileSystem.Data.WriteAllText( "Maps.json", ObjToJson( maps ) );
		}
	}
	private string ObjToJson( object o )
	{
		return JsonSerializer.Serialize( o, new JsonSerializerOptions
		{
			WriteIndented = true
		} );
	}
	public Settings set { get; set; }
	public List<Map> maps { get; set; }
}
