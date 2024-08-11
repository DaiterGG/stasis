using Sandbox;

public sealed class CameraControl : Component
{
	[Property] List<GameObject> cameras;
	int iEnabled = 0;
	protected override void OnStart()
	{
		EnableCam();
		base.OnStart();
	}
	protected override void OnFixedUpdate()
	{
		if( Input.Pressed( "CameraCycle" ) )
		{
			iEnabled += 1;
			if ( iEnabled >= cameras.Count ) iEnabled = 0;
			EnableCam();
		}

	}
	void EnableCam() { 
		for ( int i = 0; i < cameras.Count; i++ )
		{
			if ( i == iEnabled ) cameras[i].Enabled = true;
			else cameras[i].Enabled = false;
		}
	}
}
