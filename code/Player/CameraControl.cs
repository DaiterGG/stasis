namespace Sandbox.Player;
public sealed class CameraControl : Component
{
	EngineComponent ENGINE;
	[Property] public GameObject FreeCam;
	[Property] public List<GameObject> cameras;
	int iEnabled = 0;
	protected override void OnAwake()
	{
		ENGINE = Sng.Inst.Player.Engine;
	}
	protected override void OnStart()
	{
		base.OnStart();
		UpdateCam();
	}
	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "CameraCycle" ) )
		{
			iEnabled += 1;
			if ( iEnabled >= cameras.Count ) iEnabled = 0;
			UpdateCam();
		}
		if ( Input.Pressed( "FreeCamera" ) )
		{
			FreeCam.Enabled = !FreeCam.Enabled;
			UpdateCam();
			ENGINE.UpdateInput();
		}

	}
	public void UpdateCam()
	{
		var active = iEnabled;
		if ( FreeCam.Enabled )
		{
			active = -1;
		}
		for ( int i = 0; i < cameras.Count; i++ )
		{
			if ( i == iEnabled ) cameras[i].Enabled = true;
			else cameras[i].Enabled = false;
		}
	}
}
