using Sandbox.Data;

namespace Sandbox;
public sealed class Sng : Component
{
	private static Sng _sng;

	public MainTimer Timer;
	[Property] public MenuController MenuC;
	[Property] public PlayerComp Player;
	public static Sng Inst { get { return _sng; } }
	public GameObject StartPoint;

	private GameObject _spawnPoint;
	public GameObject SpawnPoint
	{
		get
		{
			if ( _spawnPoint == null ) return StartPoint;
			return _spawnPoint;
		}
		set { _spawnPoint = value; }
	}
	public List<GameObject> EndZones;

	public FileController File;
	protected override void OnAwake()
	{
		base.OnAwake();
		_sng = this;
		//LoadNewMap( "move/stasis_playground_scene" );
		MapInit();
		File = new FileController();
		File.ReadFiles();

	}
	protected override void OnStart()
	{
		base.OnStart();
		SpawnPlayer();
	}

	//unused
	private void FindMaps()
	{
		var maps = FileSystem.Mounted.FindFile( "/", "*.vpk" );
		foreach ( var map in maps )
		{
			//	Log.Info( map );
		}

	}
	private void MapInit()
	{
		Timer = new MainTimer();
		StartPoint = null;
		SpawnPoint = null;
		EndZones = new List<GameObject>();
		var heap = Scene.GetAllObjects( true );
		foreach ( var obj in heap )
		{
			//Log.Info( obj.Name );
			if ( obj.Name == "info_player_spawn" )
			{
				SpawnPoint = obj;
			}
			else if ( obj.Name == "info_player_start" )
			{
				StartPoint = obj;
			}
			else if ( obj.Name.Contains( "end_zone" ) )
			{
				EndZones.Add( obj );
			}
		}
		if ( SpawnPoint == null ) Log.Warning( "Spawn not found" );
		if ( StartPoint == null ) Log.Warning( "Start not found" );
		if ( EndZones.Count == 0 ) Log.Warning( "End zone not found" );
	}
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( MenuC.IsGaming )
		{

			if ( Input.Pressed( "Restart" ) )
			{
				ResetPlayer();
			}
			if ( Input.Pressed( "SelfDestruct" ) )
			{
				Player.SpinC.SpinCollision();
			}
			if ( Input.Pressed( "Back" ) )
			{
				MenuC.OpenMenu();
			}
			Timer.UpdateTimer();
			MenuC.IngameUI.Timer = Timer.TimerStr;
		}
	}
	public void LoadNewMap( string mapPath )
	{
		Scene.LoadFromFile( mapPath );
		MapInit();
		SpawnPlayer();
		MenuC.SetCameraLook();
	}
	public void SpawnPlayer()
	{
		Player.Transform.Position = SpawnPoint.Transform.Position;
		Player.Transform.Rotation = SpawnPoint.Transform.Rotation;
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos( true );
	}
	public void TeleportPlayer( GameTransform pos )
	{
		Player.Transform.Position = pos.Position;
		Player.Transform.Rotation = pos.Rotation;
		Player.Engine.ResetPos( false );
	}
	public void EndZoneEnter( GameObject go, Collider cof )
	{
		Timer.IsFinished = true;
	}
	public void ResetPlayer()
	{
		Player.Engine.UpdateInput();
		TeleportPlayer( StartPoint.Transform );
		Timer.TimerReset();
		Player.CameraC.FreeCam.Enabled = false;
		Player.CameraC.UpdateCam();
		Player.SpinC.RestartSpin();
	}

}

