using Sandbox;

public sealed class FreeCam : Component
{
	[Property] GameObject thirdCam;
	[Property] EngineComponent engine;
	readonly float force = 8f;
	float mult = 1f;
	protected override void OnEnabled()
	{

		Transform.Position = thirdCam.Transform.Position + new Vector3(0,0,10);
		Transform.Rotation = thirdCam.Transform.Rotation;
		engine.inputActive = false;
		base.OnEnabled();
	}
	protected override void OnDisabled()
	{
		engine.inputActive = true;
		base.OnDisabled();
	}
	protected override void OnFixedUpdate()
	{
		if ( !GameObject.Enabled ) return;
		if ( Input.Down( "Up" ) )
		{ //W or forward		
			Transform.Position += new Vector3( force * mult, 0, 0 ) * Transform.Rotation;
		}
		if (Input.Down ( "Down" ) )
		{
			Transform.Position += new Vector3( force * mult * -1, 0, 0 ) * Transform.Rotation;
		}
		if ( Input.Down ( "Left"))
		{
			Transform.Position += new Vector3( 0, force * mult, 0 ) * Transform.Rotation;
		}
		if ( Input.Down( "Right" ) )
		{
			Transform.Position += new Vector3( 0, force * mult * -1, 0 ) * Transform.Rotation;
		}
		if( Input.Down( "Jump" ) )
		{
			Transform.Position += new Vector3( 0, 0,force * mult);
		}
		if( Input.Down( "Crouch"))
		{
			Transform.Position += new Vector3( 0, 0,force * mult * -1);
			
			
		}
		if(Input.Down( "Sprint")) {
			mult = 3f;
		}
		else
		{
			mult = 1f;
		}

		var ee = Transform.Rotation.Angles();
		ee += Input.AnalogLook * 0.5f;
		ee.roll = 0;
		Transform.Rotation = ee.ToRotation();
	}
}
