using System;

namespace Sandbox.Player;
public sealed class FreeCam : Component
{
	[Property] public readonly GameObject thirdCam;
	readonly float force = 20f;
	float mult = 1f;
	MainTimer TIMER;
	EngineComponent ENGINE;
	protected override void OnAwake()
	{
		base.OnAwake();
		ENGINE = Sng.Inst.Player.Engine;
		TIMER = Sng.Inst.Timer;
	}
	protected override void OnEnabled()
	{
		base.OnEnabled();

		Transform.Position = thirdCam.Transform.Position + new Vector3( 0, 0, 10 );
		Transform.Rotation = thirdCam.Transform.Rotation;
		Transform.ClearInterpolation();
	}
	protected override void OnDisabled()
	{
		base.OnDisabled();
	}
	protected override void OnUpdate()
	{

		if ( !GameObject.Enabled ) return;
		TIMER.StopTimer();
		if ( Input.Down( "Up" ) )
		{ //W or forward		
			Transform.Position += new Vector3( force * mult, 0, 0 ) * Transform.Rotation;
		}
		if ( Input.Down( "Down" ) )
		{
			Transform.Position += new Vector3( force * mult * -1, 0, 0 ) * Transform.Rotation;
		}
		if ( Input.Down( "Left" ) )
		{
			Transform.Position += new Vector3( 0, force * mult, 0 ) * Transform.Rotation;
		}
		if ( Input.Down( "Right" ) )
		{
			Transform.Position += new Vector3( 0, force * mult * -1, 0 ) * Transform.Rotation;
		}
		if ( Input.Down( "SelfDestruct" ) )
		{
			Transform.Position += new Vector3( 0, 0, force * mult );
		}
		if ( Input.Down( "Crouch" ) )
		{
			Transform.Position += new Vector3( 0, 0, force * mult * -1 );
		}
		if ( Input.Down( "Sprint" ) )
		{
			mult = 3f;
		}
		else
		{
			mult = 1f;
		}

		var ee = Transform.Rotation.Angles();
		ee += Input.AnalogLook * Time.Delta * 3f;
		if ( Math.Abs( ee.pitch ) > 90 ) ee.pitch = 90 * Math.Sign( ee.pitch );
		ee.roll = 0;

		Transform.Rotation = ee.ToRotation();
		Transform.ClearInterpolation();
	}
}
