using System;
using Sandbox.Audio;
using Stasis.Data;
using Stasis.Player;
using Sandbox.UI;
namespace Stasis.UI;

public sealed class MenuController : Component
{
	[Property] public MainMenu MenuUI { get; set; }
	[Property] public IngameUI IngameUI { get; set; }
	[Property] public EndScreen EndUI { get; set; }
	[Property] public SettingsUI SetUI { get; set; }
	[Property] public ChooseMenu ChooseUI { get; set; }
	[Property] public ScreenPanel BlackUI { get; set; }
	[Property] public GameObject Camera { get; set; }

	public float pitchOffset = -11.3f;
	public float yawOffset = 13.111f;
	public Vector3 CameraPos;


	public float speed = 0;
	public bool IsGaming = false;
	GameObject BODY;
	SpinControl SPINC;
	Timer TIMER;
	EngineComponent ENGINE;
	Sng SNG;
	FileControl FC;

	public void OnAwakeInit()
	{
		SNG = Sng.Inst;
		SPINC = SNG.Player.SpinC;
		TIMER = SNG.Timer;
		BODY = SNG.Player.Body;
		ENGINE = SNG.Player.Engine;
		FC = SNG.FileC;
		//Set camera angle to use for all other menues
		SetCameraOffset();
		Camera.Enabled = true;
		ApplySettings();
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
		Camera.Enabled = true;
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
		TIMER.Reset();
		ShowInfo();
		SNG.ResetPlayer();
	}
	public void OpenMenu()
	{
		SNG.SpawnPlayer();
		SPINC.RestartSpin();
		IngameUI.GameObject.Enabled = false;
		if ( !Game.IsEditor ) EndUI.GameObject.Enabled = false;

		ChooseUI.GameObject.Enabled = false;
		MenuUI.GameObject.Enabled = true;
		IsGaming = false;
		CameraInit();
		Camera.Enabled = true;
	}
	public void Controls()
	{
		Game.Overlay.ShowBinds();
	}
	public void MapSelect()
	{
		try
		{
			Game.Overlay.ShowPackageSelector( "type:asset ext:scene ", delegate ( Package p )
			{
				foreach ( var m in FC.OfficialMaps )
				{
					if ( m == p.FullIdent )
					{
						FC.FetchNewMap( m, "official" );
						return;
					}
				}
				foreach ( var m in FC.FeaturedMaps )
				{
					if ( m == p.FullIdent )
					{
						FC.FetchNewMap( m, "featured" );
						return;
					}
				}
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
	public void OpenSettings()
	{
		SetUI.GameObject.Enabled = true;
		MenuUI.GameObject.Enabled = false;

	}
	public void CloseSettings()
	{
		SetUI.GameObject.Enabled = false;
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
		}
		else
		{
			EndUI.Gold = "";
			EndUI.Silver = "PASS";
			EndUI.Bronze = "";
			EndUI.medal = -1;
		}
		var strtime = SNG.FormatTime( FC.currentTime );
		EndUI.Time = strtime.Split( '.' )[0];
		EndUI.TimeMil = "." + strtime.Split( '.' )[1];
		EndUI.Scores = FC.currentMap.Scores;
		if ( FC.currentMap.Scores.Count > 0 )
		{
			if ( FC.currentMap.Scores[0].Time == FC.currentTime && FC.currentMap.Scores.Count() > 1 )
			{
				EndUI.TimeDif = ("-" + SNG.FormatTime( FC.currentMap.Scores[1].Time - FC.currentTime ));
				EndUI.timesave = true;

			}
			else
			{

				EndUI.timesave = false;
				EndUI.TimeDif = ("+" + SNG.FormatTime( FC.currentTime - FC.currentMap.Scores[0].Time ));
			}

		}
		else EndUI.TimeDif = "";

		EndUI.Author = FC.currentMap.Author;
		EndUI.Name = FC.currentMap.Name.ToUpper();

		EndUI.GameObject.Enabled = true;
	}

	public int GetMedal( MapData map )
	{
		if ( map == null || map.Scores == null || map.Scores.Count() == 0 || map.GoldTime == 0 ) return 0;
		var time = map.Scores[0].Time;
		if ( map.SpeedRun )
		{
			if ( map.GoldTime > time )
			{
				return 3;
			}
			else if ( map.SilverTime > time )
			{
				return 2;
			}
			else if ( map.BronzeTime > time )
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

	public void ApplySettings()
	{
		if ( FC.Set.Volume > 10 ) FC.Set.Volume = 10;
		Mixer.Master.Volume = FC.Set.Volume / 10f;

	}
	public void Copy()
	{
		Clipboard.SetText( "https://discord.gg/JNrNHxDE2D" );

	}
	bool infoOnce = true;
	public void ShowInfo()
	{
		if ( infoOnce && SNG.firstTime ) IngameUI.ShowInfo( ControlsInfo() );
		infoOnce = false;
	}
	private string ControlsInfo()
	{
		var w = Input.GetButtonOrigin( "Up" ).ToUpper();
		var s = Input.GetButtonOrigin( "Down" ).ToUpper();
		var a = Input.GetButtonOrigin( "Left" ).ToUpper();
		var d = Input.GetButtonOrigin( "Right" ).ToUpper();
		var p = Input.GetButtonOrigin( "SelfDestruct" ).ToUpper();
		var g = Input.GetButtonOrigin( "Back" ).ToUpper();
		var r = Input.GetButtonOrigin( "Restart" ).ToUpper();
		var t = Input.GetButtonOrigin( "CameraCycle" ).ToUpper();
		var c = Input.GetButtonOrigin( "FreeCamera" ).ToUpper();
		return $"{g} - Back to menu\n" +
			$"{w} {a} {s} {d} - To move\n" +
			$"{r} - Reset\n" +
			$"{p} - Self destruct\n" +
			$"{t} - Change view\n" +
			$"{c} - Free camera\n";

	}
}
