using Sandbox;
using System;

public sealed class Sng : Component
{
	private static Sng _sng;

	[Property] IngameUI gameUI; 
	public static Sng Inst {  get { return _sng; } }
	public GameObject SpawnPoint;
	public MainTimer Timer;
	
	protected override void OnAwake()
	{
		base.OnAwake();
		_sng = this;
		MapInit();
	}
	protected override void OnStart()
	{
		base.OnStart();

		SpawnPlayer();
	}
	private void MapInit()
	{
		Timer = new MainTimer();
		SpawnPoint = null;
		var heap = Scene.GetAllObjects( true );
		foreach ( var obj in heap )
		{
			//Log.Info( obj.Name );
			if ( obj.Name == "info_player_start" )
			{
				SpawnPoint = obj;
			}
		}
		if ( SpawnPoint == null ) Log.Info( "Spawn not found" );
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
		PlayerSng.Player.Self.Transform.Position =
			SpawnPoint.Transform.Position;
		PlayerSng.Player.Self.Transform.Rotation = SpawnPoint.Transform.Rotation;
		PlayerSng.Player.Engine.ResetPos(true);
		Log.Info( "Player Spawned" );
	}
	public void TeleportPlayer(GameTransform pos )
	{
		PlayerSng.Player.Self.Transform.Position = pos.Position;
		PlayerSng.Player.Self.Transform.Rotation = pos.Rotation;
		PlayerSng.Player.Engine.ResetPos(false);
		Log.Info( "Player Teleported" );
	}
	public void EndZoneEnter(GameObject go,Collider cof )
	{
		Timer.IsFinished = true;
	}






}

