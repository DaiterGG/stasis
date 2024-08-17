using System;

namespace Sandbox;

public sealed class MenuController : Component
{
    [Property] public MainMenu Menu { get; set; }
    [Property] public GameObject Camera { get; set; }
	public float pitchOffset = -11.3f;
	public float yawOffset = 13.111f;
	public float speed = 0;
	public Vector3 CameraPos;
	public bool IsGaming = false;
	GameObject BODY; 
	IngameUI GAMEUI;
	EngineComponent ENGINE;
	Sng SNG;
	
	protected override void OnAwake()
	{
		base.OnAwake();
		SNG = Sng.Inst;
		BODY = Sng.Inst.Player.Body;
		GAMEUI = Sng.Inst.gameUI;
		ENGINE = Sng.Inst.Player.Engine;
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
		if ( Input.Down( "attack1" ))
        {
			speed += Mouse.Delta.x / 2000f;
        }	

            var rad = Camera.Transform.Position - BODY.Transform.Position;
            var sin = (float)Math.Sin( speed);
            var cos = (float)Math.Cos( speed);
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
            Camera.Transform.Rotation = Rotation.LookAt( rad * new Vector3( -1, -1, 0)) * Rotation.FromYaw( yawOffset) * Rotation.FromPitch( pitchOffset );

    }
	public void Play()
	{
		Camera.Enabled = false;
		ENGINE.inputActive = true;
		GAMEUI.GameObject.Enabled = true;
		Menu.Enabled = false;
		IsGaming = true;
		SNG.ResetPlayer();
	}
	public void OpenMenu()
	{
		Camera.Enabled = true;
		ENGINE.inputActive = false;
		GAMEUI.GameObject.Enabled = false;
		Menu.Enabled = true;
		IsGaming = false;

	}
	public void OpenSetting()
	{
		
	}
	public void OpenMapSelect()
	{
			
	}
}
