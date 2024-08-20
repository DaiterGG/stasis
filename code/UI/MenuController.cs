using Sandbox.Data;
using System;
using System.Text.Json;
namespace Sandbox;

public sealed class MenuController : Component
{
	[Property] public MainMenu MenuUI { get; set; }
	[Property] public IngameUI IngameUI { get; set; }
	[Property] public GameObject Camera { get; set; }
	public float pitchOffset = -11.3f;
	public float yawOffset = 13.111f;
	public float speed = 0;
	public Vector3 CameraPos;
	public bool IsGaming = false;
	GameObject BODY;
	SpinController SPINC;
	EngineComponent ENGINE;
	Sng SNG;

	protected override void OnAwake()
	{
		base.OnAwake();
		SNG = Sng.Inst;
		SPINC = SNG.Player.SpinC;
		BODY = SNG.Player.Body;
		ENGINE = SNG.Player.Engine;
	}
	protected override void OnStart()
	{
		base.OnStart();
		CameraPos = Camera.Transform.Position - BODY.Transform.Position;
		CameraInit();
		OpenMenu();
	}
	protected override void OnUpdate()
	{
		if ( Input.Down( "attack1" ) )
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
	public void CameraInit()
	{
		Camera.Transform.Position = BODY.Transform.Position + CameraPos;
		SetCameraLook();
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
		MenuUI.Enabled = false;
		IsGaming = true;
		ShowInfo();
		SNG.ResetPlayer();
	}
	public void OpenMenu()
	{
		SNG.SpawnPlayer();
		SPINC.RestartSpin();
		IngameUI.GameObject.Enabled = false;
		MenuUI.Enabled = true;
		IsGaming = false;
		ENGINE.UpdateInput();
		Camera.Enabled = true;
	}
	public void Controls()
	{
		Game.Overlay.ShowBinds();
	}
	public void Options()
	{
		Package gg = new Package();
		Game.Overlay.ShowPackageSelector( "options", delegate ( Package p )
		{
			Log.Info( $"Package selected: {p.Url}" );
		} );
	}

	public void MapSelect()
	{

		//FileSystem.Data.WriteAllText( "userdata.txt", "" );
		var contr = new FileController();

		Log.Info( JsonSerializer.Serialize( contr, new JsonSerializerOptions
		{
			WriteIndented = true
		} ) );
		FileSystem.Data.WriteJson( "userdata.txt", contr );
		//Log.Info( contr.set.MouseInvertX.ToString() );
		//	Log.Info( FileSystem.Data.GetFullPath( "userdata" ) );

		Game.Overlay.ShowPackageSelector( "type:asset ext:scene stasis_map", delegate ( Package p )
		{
			Scene.LoadFromFile( p.FullIdent );

		} );
	}
	public void Quit()
	{
		Game.Close();
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
