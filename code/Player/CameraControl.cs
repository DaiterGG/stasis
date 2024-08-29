namespace Sandbox.Player;
public sealed class CameraControl : Component
{
	EngineComponent ENGINE;
	[Property] public FreeCam FreeCam;
	[Property] public List<GameObject> cameras;
	int iEnabled = 0;
	public void OnAwakeInit()
	{

		ENGINE = Sng.Inst.Player.Engine;
		FreeCam.OnAwakeInit();
		UpdateCam();
	}
	public void OnFixedGlobal()
	{
		if ( Input.Pressed( "CameraCycle" ) )
		{
			iEnabled += 1;
			if ( iEnabled >= cameras.Count ) iEnabled = 0;
			UpdateCam();
		}
		if ( Input.Pressed( "FreeCamera" ) )
		{
			FreeCam.GameObject.Enabled = !FreeCam.GameObject.Enabled;
			UpdateCam();
		}

	}
	public void UpdateCam()
	{
		var active = iEnabled;
		if ( FreeCam.GameObject.Enabled )
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
