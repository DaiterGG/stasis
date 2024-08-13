using Sandbox;

public sealed class CameraControl : Component
{
	[Property] GameObject FreeCam;
	[Property] List<GameObject> cameras;
	int iEnabled = 0;
	bool IsFreeCam = false;
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
		if ( Input.Pressed( "FreeCam" ) )
		{
			FreeCam.Enabled = !FreeCam.Enabled;
			IsFreeCam = FreeCam.Enabled;
			EnableCam();
		}


	}
	void EnableCam() {
		var active = iEnabled;
		if (IsFreeCam) //not nessary because of cameras priority queue
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
