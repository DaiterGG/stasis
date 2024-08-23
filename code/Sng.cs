using System;
using Sandbox.Data;
using Sandbox.Player;

namespace Sandbox;
public sealed class Sng : Component
{
	private static Sng _sng;

	public MainTimer Timer;
	[Property] public MenuController MenuC;
	[Property] public PlayerComp Player;
	[Property] public ZoneCreate ZoneC;
	[Property] public FileController File;
	public static Sng Inst { get { return _sng; } }

	public GameTransform StartPoint;
	private GameTransform _spawnPoint;
	public GameTransform SpawnPoint
	{
		get
		{
			if ( _spawnPoint == null ) return StartPoint;
			return _spawnPoint;
		}
		set { _spawnPoint = value; }
	}
	public List<GameObject> EndZones;
	protected override void OnAwake()
	{
		_sng = this;
		Timer = new MainTimer();
		base.OnAwake();
		File.ReadFiles();
	}
	protected override void OnStart()
	{
		File.AddOfficialMaps();
		//File.FetchNewMap( "move.stasis_playground_scene", "official" );
		//File.FetchNewMap( "dicta.base", "community" );
		File.DownloadAndLoad( "move.stasis_playground_scene", true );
		// File.DownloadAndLoad( "dicta.base" );
		base.OnStart();
	}

	private void MapInit()
	{
		Info MapInfo = null;
		Timer = new MainTimer();
		//StartPoint = null;
		//SpawnPoint = null;
		EndZones = new List<GameObject>();
		var heap = Scene.GetAllObjects( true );
		foreach ( var obj in heap )
		{
			if ( obj.Name == "info_player_spawn" )
			{
				SpawnPoint = obj.Transform;
			}
			else if ( obj.Name == "info_player_start" )
			{
				StartPoint = obj.Transform;
			}
			else if ( obj.Name.Contains( "end_zone" ) )
			{
				EndZones.Add( obj );
			}
			else if ( obj.Name == "Map Info" )
			{
				MapInfo = obj.Components.Get<Info>();
				Log.Info( "info found" );
			}
		}
		if ( MapInfo == null ) Log.Warning( "Info not found" );

		File.InfoSerialize( MapInfo );

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
		try
		{
			MenuC.BlackUI.Opacity -= 0.005f;
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
		catch ( Exception e ) { Log.Warning( e.Message ); }
	}
	public void LoadNewMap( SceneFile file, bool playground = false )
	{
		if ( playground )
		{
			var m = Cloud.Asset( "move.stasis_playground_scene" );
			Scene.Load( file );
		}
		else
		{

			Log.Info( "Map trying to load: " + file.ResourceName );
			try
			{
				Scene.Load( file );
			}
			catch ( Exception e ) { Log.Warning( "Map not found localy: " + e.Message ); }
		}

		MapInit();
		MenuC.OpenMenu();
	}
	public void SpawnPlayer()
	{
		if ( SpawnPoint == null )
		{
			Log.Warning( "No spawn and no start point" );
			SpawnPoint = Player.Transform;
			StartPoint = Player.Transform;
		}
		Player.Transform.Position = Transform.Position;
		Player.Transform.Rotation = Transform.Rotation;
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos( true );
	}
	public void TeleportPlayer( GameTransform pos )
	{
		if ( SpawnPoint == null )
		{
			Log.Warning( "No spawn and no start point" );
			SpawnPoint = Player.Transform;
			StartPoint = Player.Transform;
		}
		if ( pos == null )
		{
			try
			{
				Player.Transform.Position = SpawnPoint.Position;
				Player.Transform.Rotation = SpawnPoint.Rotation;
			}
			catch { }
		}
		else
		{
			Player.Transform.Position = pos.Position;
			Player.Transform.Rotation = pos.Rotation;
		}
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos( false );
	}
	public void EndZoneEnter( GameObject go, Collider cof )
	{
		if ( Timer.IsFinished ) return;
		File.SetScore( Timer.timerSeconds );
		Timer.IsFinished = true;
		MenuC.ShowEndScreen();

	}
	public void ResetPlayer()
	{
		Player.Engine.UpdateInput();
		TeleportPlayer( StartPoint );
		Timer.TimerReset();
		Player.CameraC.FreeCam.Enabled = false;
		Player.CameraC.UpdateCam();
		Player.SpinC.RestartSpin();
	}
	public string FormatTime( float totalSeconds )
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds( totalSeconds );

		string formattedTime = "";

		if ( timeSpan.Hours > 0 )
		{
			formattedTime += $"{timeSpan.Hours:D2}:";
		}
		else if ( timeSpan.Minutes > 0 || timeSpan.Hours > 0 )
		{
			formattedTime += $"{timeSpan.Minutes:D2}:";
		}
		formattedTime += $"{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";

		if ( formattedTime[0] == '0' && formattedTime[1] != '.' )
			formattedTime = formattedTime.Substring( 1 );

		return formattedTime;
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

