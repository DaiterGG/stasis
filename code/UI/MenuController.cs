using System;
using Sandbox.Data;
using Sandbox.Player;
namespace Sandbox;

public sealed class MenuController : Component
{
	[Property] public MainMenu MenuUI { get; set; }
	[Property] public IngameUI IngameUI { get; set; }
	[Property] public EndScreen EndUI { get; set; }
	[Property] public ChooseMenu ChooseUI { get; set; }
	[Property] public GameObject Camera { get; set; }

	public float pitchOffset = -11.3f;
	public float yawOffset = 13.111f;
	public Vector3 CameraPos;


	public float speed = 0;
	public bool IsGaming = false;
	GameObject BODY;
	SpinController SPINC;
	EngineComponent ENGINE;
	Sng SNG;
	FileController FC;

	protected override void OnAwake()
	{
		base.OnAwake();
		SNG = Sng.Inst;
		SPINC = SNG.Player.SpinC;
		BODY = SNG.Player.Body;
		ENGINE = SNG.Player.Engine;
		FC = SNG.File;
		SetCameraOffset();
	}
	protected override void OnStart()
	{
		base.OnStart();
	}
	protected override void OnUpdate()
	{
		if ( Input.Down( "attack1" ) || Input.Down( "attack2" ) )
		{
			speed += Mouse.Delta.x / 2000f;
		}

		var rad = Camera.Transform.Position - BODY.Transform.Position;
		var sin = (float)Math.Sin( speed );
		var cos = (float)Math.Cos( speed );
		var x = rad.x * cos - rad.y * sin;
		var y = rad.x * sin + rad.y * cos;

		Camera.Transform.Position = new Vector3( x, y, Camera.Transform.Position.z ) + BODY.Transform.Position * new Vector3( 1, 1, 0 );
		//Camera.Transform.Rotation = Rotation.FromYaw(Mouse.Delta.x / 100) + Camera.Transform.Rotation;
		SetCameraLook();
		speed *= 0.93f;
	}
	public void SetCameraOffset()
	{
		CameraPos = Camera.Transform.LocalPosition;
	}
	public void CameraInit()
	{
		Camera.Transform.LocalPosition = CameraPos;

		SetCameraLook();
		Camera.Transform.ClearInterpolation();
	}
	public void SetCameraLook()
	{
		var rad = Camera.Transform.Position - BODY.Transform.Position;
		Camera.Transform.Rotation = Rotation.LookAt( rad * new Vector3( -1, -1, 0 ) ) * Rotation.FromYaw( yawOffset ) * Rotation.FromPitch( pitchOffset );

	}
	public void Play()
	{
		Camera.Enabled = false;
		IngameUI.GameObject.Enabled = true;
		MenuUI.GameObject.Enabled = false;
		IsGaming = true;
		ShowInfo();
		SNG.ResetPlayer();
	}
	public void OpenMenu()
	{
		SNG.SpawnPlayer();
		SPINC.RestartSpin();
		IngameUI.GameObject.Enabled = false;
		//EndUI.GameObject.Enabled = false;
		MenuUI.GameObject.Enabled = true;
		IsGaming = false;
		CameraInit();
		Camera.Enabled = true;
		ENGINE.UpdateInput();
	}
	public void Controls()
	{
		Game.Overlay.ShowBinds();
	}
	public void Options()
	{

	}
	public void MapSelect()
	{
		try
		{
			Game.Overlay.ShowPackageSelector( "type:asset ext:scene stasis_map", delegate ( Package p )
			{
				FC.FetchNewMap( p.FullIdent, "community" );
			} );
		}
		catch ( Exception e ) { Log.Warning( e ); }
	}
	public void OpenChooseMenu()
	{
		UpdateMapsList();
		ChooseUI.GameObject.Enabled = true;
		MenuUI.GameObject.Enabled = false;

	}
	public void CloseChooseMenu()
	{
		ChooseUI.GameObject.Enabled = false;
		MenuUI.GameObject.Enabled = true;
	}
	public void UpdateMapsList()
	{
		ChooseUI.Official.Clear();
		ChooseUI.Featured.Clear();
		ChooseUI.Community.Clear();
		foreach ( var m in FC.Maps )
		{
			if ( m.Type == "official" )
			{
				ChooseUI.Official.Add( m );
			}
			else if ( m.Type == "featured" )
			{
				ChooseUI.Featured.Add( m );
			}
			else
			{
				ChooseUI.Community.Add( m );
			}
			//ChooseUI.Official.Add( m );
			//ChooseUI.Community.Add( m );
		}
	}
	public void Quit()
	{
		Game.Close();
	}
	public void ShowEndScreen()
	{
		if ( FC.currentMap == null )
		{
			Log.Warning( "no map info loaded?, why there is end_zone?" );
			return;
		}
		if ( FC.currentTime == 0 )
		{
			Log.Warning( "currentTime = 0, no map info loaded?, why there is end_zone?" );
			return;
		}
		var time = FC.currentMap.Scores[0].Time;
		if ( FC.currentMap.SpeedRun )
		{
			EndUI.Gold = SNG.FormatTime( FC.currentMap.GoldTime );
			EndUI.Silver = SNG.FormatTime( FC.currentMap.SilverTime );
			EndUI.Bronze = SNG.FormatTime( FC.currentMap.BronzeTime );

			EndUI.medal = GetMedal( FC.currentMap );
			EndUI.img = EndUI.medal.ToString();
		}
		else
		{
			EndUI.Gold = "";
			EndUI.Silver = "PASS";
			EndUI.Bronze = "";
			EndUI.medal = 2;
			EndUI.img = "3";
		}
		var strtime = SNG.FormatTime( FC.currentTime );
		EndUI.Time = strtime.Split( '.' )[0];
		EndUI.TimeMil = "." + strtime.Split( '.' )[1];
		EndUI.Scores = FC.currentMap.Scores;
		if ( FC.currentMap.Scores.Count() > 0 )
		{
			var dif = FC.currentMap.Scores[0].Time - FC.currentTime;
			EndUI.TimeDif = (dif < 0 ? "-" : "+") + SNG.FormatTime( dif );
			EndUI.timesave = dif < 0;

		}
		else EndUI.TimeDif = "";

		EndUI.Author = FC.currentMap.Author;
		EndUI.Name = FC.currentMap.Name.ToUpper();

		EndUI.GameObject.Enabled = true;
	}

	public int GetMedal( MapData map )
	{
		if ( map.SpeedRun == null ) return 0;
		if ( map.Scores.Count() == 0 ) return 0;
		var time = map.Scores[0].Time;
		if ( map.SpeedRun )
		{
			if ( FC.currentMap.GoldTime > time )
			{
				return 3;
			}
			else if ( FC.currentMap.SilverTime > time )
			{
				return 2;
			}
			else if ( FC.currentMap.BronzeTime > time )
			{
				return 1;

			}

		}
		else
		{
			return 3;
		}
		return 0;

	}


	public void ShowInfo()
	{
		IngameUI.ShowInfo( ControlsInfo() );
	}
	private string ControlsInfo()
	{
		var w = Input.GetButtonOrigin( "Up" ).ToUpper();
		var s = Input.GetButtonOrigin( "Down" ).ToUpper();
		var a = Input.GetButtonOrigin( "Left" ).ToUpper();
		var d = Input.GetButtonOrigin( "Right" ).ToUpper();
		var p = Input.GetButtonOrigin( "SelfDestruct" ).ToUpper();
		var g = Input.GetButtonOrigin( "Back" ).ToUpper();
		var c = Input.GetButtonOrigin( "FreeCamera" ).ToUpper();
		return $"{g} - Back to menu\n" +
			$"{w} {a} {s} {d} - To move\n" +
			$"{g} - Reset\n" +
			$"{p} - Self destruct\n" +
			$"1-3 - Change view\n" +
			$"{c} - Free camera\n";

	}
}
