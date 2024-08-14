using Sandbox;
using System;
using System.Numerics;
public sealed class EngineComponent : Component
{
	MainTimer TIMER = Sng.Inst.Timer;

	public bool inputActive = true;
	public bool isRunning;
	public int isStarting = 0;
	[Property] Rigidbody rigid;
	IngameUI GAMEUI = Sng.Inst.gameUI;

	[Property, Range( 0, 500f ), DefaultValue( 15f )] readonly float bodyOffsetZ;
	[Property, Range( 0, 2000f ), DefaultValue( 1400f )] readonly float turnSpeed;
	[Property, Range( 0, 20000f ), DefaultValue( 6000f )] readonly float gravity;
	[Property, Range( 0, 2000f ), DefaultValue( 100f )] readonly float gainStep;
	[Property, Range( 0, 500f ), DefaultValue( 100f )] readonly float settleStep;
	//[Property, Range( 0, 50000f ), DefaultValue( 1000f )] readonly float maxSpeed;
	float gain = 0f;
	int invertVert = -1;
	float maxGain;
	int engineDelay = 1000;
	//exp
	[Property, Range( 1f, 300f, 1f ), DefaultValue( 1f )] readonly float boost = 1.1f;
	float deltaZ = 0;
	protected override void OnStart()
	{
		base.OnStart();
		maxGain = gravity * 1.08f;
		ResetPos( true );
	}

	protected override void OnFixedUpdate()
	{

		if ( !inputActive ) return;
		var dx = Mouse.Delta.x == 0 ? 0 : Mouse.Delta.x / Math.Abs( Mouse.Delta.x ) * 1200 * .5f;
		var dy = Mouse.Delta.y == 0 ? 0 : Mouse.Delta.y / Math.Abs( Mouse.Delta.y ) * 1200;
		var dz = 0f;

		if ( Input.Down( "Left" ) )
		{
			dz += turnSpeed;
		}
		if ( Input.Down( "Right" ) )
		{
			dz -= turnSpeed;
		}
		if ( Input.Down( "Down" ) )
		{
			gain -= gravity / gainStep;
		}
		if ( Input.Down( "Up" ) )
		{
			if ( !isRunning ) engStart();
			else gain += gravity / gainStep;
		}
		if ( !Input.Down( "Up" ) && !Input.Down( "Down" ) )
		{
			gain -= (gain - gravity) / settleStep;
		}
		if ( !isRunning )
		{
			dx = 0f; dy = 0f; dz = 0f; gain = 0;
		}
		if ( gain > maxGain ) gain = maxGain;
		if ( gain < maxGain * -1 ) gain = maxGain * -1;

		Vector3 gainAng = new Vector3( 0, 0, gain ) * Transform.Rotation;
		//if horizontal velocity is too high
		//if (new Vector3(rigid.Velocity.x,rigid.Velocity.y, 0).Length > maxSpeed)
		//{
		//if same direction
		//if (gainAng.x * rigid.Velocity.x >0) gainAng *= new Vector3(0, 1, 1);
		//if (gainAng.y * rigid.Velocity.y >0) gainAng *= new Vector3(1, 0, 1);
		//}
		rigid.ApplyForce( gainAng );
		rigid.ApplyTorque( new Vector3( dx, dy * invertVert, dz ) * Transform.Rotation );
		if (!isRunning) rigid.ApplyForce( new Vector3( 0, 0, gravity * -1 * .05f ) );
		else rigid.ApplyForce( new Vector3( 0, 0, gravity * -1  ) );
		//experimental
		//rigid.ApplyForce( new Vector3(rigid.Velocity.x * boost, rigid.Velocity.y * boost, 0 ) * Transform.Rotation);

		deltaZ -= Transform.Position.z;
		if ( deltaZ < 0 ) deltaZ = 0;
		else deltaZ = Math.Abs( deltaZ ) * boost;
		rigid.ApplyForce( new Vector3(
			Math.Sign( rigid.Velocity.x ) * deltaZ
			, Math.Sign( rigid.Velocity.y ) * deltaZ
			, 0 ) * Transform.Rotation );
		deltaZ = Transform.Position.z;

		GAMEUI.SpeedMain = ((int)rigid.Velocity.Length).ToString();
		GAMEUI.SpeedVert = ((int)rigid.Velocity.z).ToString();
		GAMEUI.SpeedHor = ((int)Math.Sqrt(
			rigid.Velocity.x * rigid.Velocity.x +
			rigid.Velocity.y * rigid.Velocity.y
			)).ToString();
	}

	private void MyEvent( object sender, EventArgs e )
	{
		throw new NotImplementedException();
	}

	public void ResetPos( bool engineRestart )
	{
		GameObject.Transform.Position = GameObject.Parent.Transform.Position + new Vector3( 0, 0, bodyOffsetZ );
		GameObject.Transform.Rotation = GameObject.Parent.Transform.Rotation;
		rigid.Velocity = new Vector3( 0 );
		rigid.AngularVelocity = new Vector3( 0 );
		if ( engineRestart )
		{
			engOff();
		}
		else if ( isRunning ) gain = gravity;
	}
	public void engOff()
	{
		isStarting = 0;
		isRunning = false;
		TIMER.IsAirborne = false;
		gain = 0f;
	}
	public void engStart()
	{
		isStarting += 1;
		if ( isStarting > 100 )
		{
			isStarting = 0;
			isRunning = true;
			TIMER.IsAirborne = true;
			gain = gravity;
		}
	}
}
