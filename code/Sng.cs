using Sandbox.Player;
using System;

public sealed class Sng : Component
{
	private static Sng _sng;

	[Property] IngameUI gameUI; 
	public static Sng Inst {  get { return _sng; } }
	public GameObject SpawnPoint;
	public PlayerComp Player;
	public MainTimer Timer;
	
	protected override void OnAwake()
	{
		base.OnAwake();
		_sng = this;
		MapInit();
		SpawnPlayer();
	}
	protected override void OnStart()
	{
		base.OnStart();
	}
	private void MapInit()
	{
		Timer = new MainTimer();
		Player = null;
		SpawnPoint = null;
		var heap = Scene.GetAllObjects( true );
		foreach ( var obj in heap )
		{
			//Log.Info( obj.Name );
			if ( obj.Name == "info_player_start" )
			{
				SpawnPoint = obj;
			}
			if ( obj.Name == "Player" )
			{
				if ( Player != null ) Log.Info( "second player found" );
				Player = new PlayerComp( obj );
			}
		}
		if ( SpawnPoint == null ) Log.Info( "Spawn not found" );
		if ( Player == null ) Log.Error( "Player not found!" );
	}
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if(Input.Pressed( "Restart" ) )
		{
			TeleportPlayer( SpawnPoint.Transform );
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
	}
private void SpawnPlayer()
	{
		Player.self.Transform.Position = SpawnPoint.Transform.Position;
		Player.self.Transform.Rotation = SpawnPoint.Transform.Rotation;
		Player.engine.ResetPos(true);
		Log.Info( "Player Spawned" );
	}
	public void TeleportPlayer(GameTransform pos )
	{
		Player.self.Transform.Position = pos.Position;
		Player.self.Transform.Rotation = pos.Rotation;
		Player.engine.ResetPos(false);
		Log.Info( "Player Teleported" );
	}
	public void EndZoneEnter(GameObject go,Collider cof )
	{
		Timer.IsFinished = true;
	}






}

