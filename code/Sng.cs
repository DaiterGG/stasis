using System;
using Sandbox.Data;
using Sandbox.Player;
namespace Sandbox;
public sealed class Sng : Component
{
	private static Sng _sng;
	public static Sng Inst { get { return _sng; } }

	public MainTimer Timer;
	[Property] public MenuController MenuC;
	[Property] public PlayerComp Player;
	[Property] public ZoneCreate ZoneC;
	[Property] public FileController File;
	public bool firstTime { get; set; } = true;
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
	public static void ELog( object s )
	{
		if ( Game.IsEditor ) Log.Info( s );
	}
	string lastLog;
	public static void LogOnce( object s, bool editLog = true )
	{
		if ( Inst.lastLog != s.ToString() )
		{
			if ( editLog )
			{
				ELog( s );
			}
			else Log.Info( s );
			Inst.lastLog = s.ToString();
		}
	}
	protected override void OnAwake()
	{
		if ( Game.IsEditor && Scene.Name != "Scene" )
		{
			_sng = this;
			ELog( "testing in editor" );
		}
		else if ( GameObject.Parent.Name == "Menu" )
		{
			_sng = this;
		}
		else
		{
			GameObject.Parent.Enabled = false;
			return;
		}

		OnAwakeInit();
	}
	public void OnAwakeInit()
	{
		Timer = new MainTimer();
		File.OnAwakeInit();
		MenuC.OnAwakeInit();
		Player.Engine.OnAwakeInit();
		Player.SoundC.OnAwakeInit();
		Player.SpinC.OnAwakeInit();
		ZoneC.OnAwakeInit();
		Player.CameraC.OnAwakeInit();

	}

	protected override void OnStart()
	{
		File.AddOfficialMaps();
		//File.FetchNewMap( "move.stasis_playgr", "official" );
		//File.FetchNewMap( "dicta.base", "community" );
		//File.DownloadAndLoad( "move.stasis_playground", true );

		if ( Game.IsEditor )
		{
			File.SetCurrentMap( "move.plground" );
			MapInit();
		}
		else
		{
			File.DownloadAndLoad( "move.plground" );
		}

		base.OnStart();
	}
	private void MapInit()
	{
		Info MapInfo = null;
		StartPoint = null;
		SpawnPoint = null;
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
				ELog( "Info Found" );
			}
		}
		if ( MapInfo == null ) Log.Warning( "Info not found" );

		File.InfoSerialize( MapInfo );

		if ( EndZones.Count == 0 ) Log.Warning( "End zone not found" );
		else ZoneC.MapInit();

		if ( StartPoint == null ) Log.Warning( "Start not found" );
		SpawnPlayer();
	}
	protected override void OnFixedUpdate()
	{
		OnFixedUpdateSequence();
	}
	private void OnFixedUpdateSequence()
	{
		try
		{
			MenuC.BlackUI.Opacity -= 0.005f;
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
				if ( Input.Pressed( "Back" ) || Input.EscapePressed )
				{
					MenuC.OpenMenu();
				}
			}
			Player.SoundC.OnFixedGlobal();
			Player.SpinC.OnFixedGlobal();
			Player.Engine.OnFixedGlobal();
			if ( MenuC.IsGaming ) Timer.OnFixedGlobal();
			Player.CameraC.OnFixedGlobal();
		}
		catch ( Exception e ) { LogOnce( "Main fixed error" + e.StackTrace ); }
	}

	public void LoadNewMap( SceneFile file )
	{

		Log.Info( "Map trying to load: " + file.ResourceName );
		try
		{
			Scene.Load( file );
		}
		catch ( Exception e ) { Log.Warning( "Map not found localy: " + e.Message ); }

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
		Player.Transform.Position = SpawnPoint.Position;
		Player.Transform.Rotation = SpawnPoint.Rotation;
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos( true );
		MenuC.SetCameraLook();
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
			Player.Transform.Position = SpawnPoint.Position;
			Player.Transform.Rotation = SpawnPoint.Rotation;
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
		Timer.TimerFinish();
		MenuC.ShowEndScreen();

	}
	public void ResetPlayer()
	{
		TeleportPlayer( StartPoint );
		Timer.Reset();
		Player.CameraC.FreeCam.GameObject.Enabled = false;
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

