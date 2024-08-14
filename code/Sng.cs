using Sandbox;
using System;

public sealed class Sng : Component
{
	private static Sng _sng;

	[Property] public IngameUI gameUI;
	[Property] public MenuController menuUI;
	[Property] public PlayerComp Player;
	public static Sng Inst {  get { return _sng; } }
	public GameObject StartPoint;

	private GameObject _spawnPoint;
	protected override void OnAwake()
	{
		base.OnAwake();
		_sng = this;
		FindMaps();
		MapInit();
	}
	protected override void OnStart()
	{
		base.OnStart();
		SpawnPlayer();
	}


	public GameObject SpawnPoint { get
		{
			if ( _spawnPoint == null ) return StartPoint;
			return _spawnPoint;
		}
		set { _spawnPoint = value;}
	}
	public List<GameObject> EndZones;

	public MainTimer Timer;
	
	private void FindMaps()
	{
		var maps = FileSystem.Mounted.FindFile( "/maps", "*.vpk" );
		foreach ( var map in maps )
		{
			Log.Info( map );
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
			}else if ( obj.Name == "info_player_start" )
			{
				StartPoint = obj;
			}else if( obj.Name.Contains("end_zone"))
			{
				EndZones.Add( obj );
			}
		}
		if ( SpawnPoint == null ) Log.Info( "Spawn not found" );
		if ( StartPoint == null ) Log.Info( "Start not found" );
		if ( EndZones.Count == 0 ) Log.Info( "End zone not found" );
	}
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if(Input.Pressed( "Restart" ) )
		{
			TeleportPlayer( StartPoint.Transform );
			Timer.TimerReset();
		}
		if ( Input.Pressed( "Test" ) )
		{
			LoadNewMap( "scenes/mapload.scene" );
		}
		Timer.UpdateTimer();
		gameUI.Timer = Timer.TimerStr;
	}
	public void LoadNewMap( string mapPath )
	{
		Scene.LoadFromFile( mapPath );
		MapInit();
		SpawnPlayer();
		//menuUI.SetCameraLook();
	}
private void SpawnPlayer()
	{
		Player.GameObject.Transform.Position =
			SpawnPoint.Transform.Position;
		Player.GameObject.Transform.Rotation = SpawnPoint.Transform.Rotation;
		Player.Engine.ResetPos(true);
		Log.Info( "Player Spawned" );
	}
	public void TeleportPlayer(GameTransform pos )
	{
		Player.GameObject.Transform.Position = pos.Position;
		Player.GameObject.Transform.Rotation = pos.Rotation;
		Player.Engine.ResetPos(false);
		Log.Info( "Player Teleported" );
	}
	public void EndZoneEnter(GameObject go,Collider cof )
	{
		Timer.IsFinished = true;
	}
}

