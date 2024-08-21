using Sandbox.Data;

namespace Sandbox;
public sealed class Sng : Component
{
	private static Sng _sng;

	public MainTimer Timer;
	[Property] public MenuController MenuC;
	[Property] public PlayerComp Player;
	[Property] public ZoneCreate ZoneC;
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
	public Info MapInfo;
	public string MapIndent;

	public FileController File;
	protected override void OnAwake()
	{
		_sng = this;
		base.OnAwake();
		File = new FileController();
		File.ReadFiles();
	}
	protected override void OnStart()
	{
		LoadNewMap( "move.stasis_playground_scene" );
		base.OnStart();
	}

	private void MapInit()
	{
		Timer = new MainTimer();
		StartPoint = null;
		MapInfo = null;
		SpawnPoint = null;
		EndZones = new List<GameObject>();
		var heap = Scene.GetAllObjects( true );
		foreach ( var obj in heap )
		{
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
			else if ( obj.Name == "Map Info" )
			{
				MapInfo = obj.Components.Get<Info>();
				Log.Info( MapInfo.DisplayName );
			}
		}
		if ( MapInfo == null ) Log.Warning( "Info not found" );
		else File.MapInfoSerialize( MapInfo );

		if ( EndZones.Count == 0 ) Log.Warning( "End zone not found" );
		else ZoneC.MapInit();

		if ( StartPoint == null ) Log.Warning( "Start not found" );
		else
		{
			MenuC.SetCameraLook();
			SpawnPlayer();
		}
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
		MapIndent = mapPath;
		var str = Cloud.Asset( "move.stasis_playground_scene" );
		// Package.
		Scene.LoadFromFile( str );
		MapInit();
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
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos( false );
	}
	public void EndZoneEnter( GameObject go, Collider cof )
	{
		Timer.IsFinished = true;
		MenuC.EndUI.GameObject.Enabled = true;
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
	//unused
	private void FindMaps()
	{
		var maps = FileSystem.Mounted.FindFile( "/", "*.vpk" );
		foreach ( var map in maps )
		{
			//	Log.Info( map );
		}

	}

}

