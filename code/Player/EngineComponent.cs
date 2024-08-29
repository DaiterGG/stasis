using System;
using Sandbox.Data;

namespace Sandbox.Player;
public sealed class EngineComponent : Component
{
	IngameUI GAMEUI;
	MenuController MENUC;
	FreeCam FREECAM;
	FileController FC;
	SpinControl SPINC;

	public bool isRunning;
	public int progress;
	public Rigidbody rigid;
	[Property, Range( 0.9f, 1f, 0.001f )] public float horizontalDumping { get; private set; } = 0.978f;
	[Property, Range( 0.9f, 1f, 0.001f )] public float verticalDumping { get; private set; } = 0.968f;
	[Property, Range( 0, 500f )] public float bodyOffsetZ { get; private set; } = 0f;
	[Property, Range( 0, 20000f )] public float turnSpeed { get; private set; } = 16000f;
	[Property, Range( 0, 20000f )] public float gravity { get; private set; } = 40f;
	[Property, Range( 0, 2000f )] public float gainStep { get; private set; } = 100f;
	[Property, Range( 0, 500f )] public float settleStep { get; private set; } = 100f;
	//[Property, Range( 0, 50000f ), DefaultValue( 1000f )] readonly float maxSpeed;
	public float gain { get; private set; } = 0f;
	float maxGainGravityScale = 1.15f;
	public float maxGain { get { return gravity * maxGainGravityScale; } }
	int invertVert = -1;
	public bool inputActive
	{
		get
		{
			return !(FREECAM.GameObject.Enabled || !SPINC.isAttached || !MENUC.IsGaming);
		}
		set { }
	}

	//exp
	[Property, Range( 1f, 300f, 1f ), DefaultValue( 1f )] public readonly float boost = 1.1f;
	float deltaZ = 0;
	//


	public void OnAwakeInit()
	{

		FREECAM = Sng.Inst.Player.CameraC.FreeCam;
		SPINC = Sng.Inst.Player.SpinC;
		MENUC = Sng.Inst.MenuC;
		GAMEUI = MENUC.IngameUI;
		FC = Sng.Inst.File;
		rigid = GameObject.Components.Get<Rigidbody>();
		Log.Info( rigid );
		ResetPos( true );
	}
	public void OnFixedGlobal()
	{

		//Log.Info( FREECAM.Enabled + " " + !SPINC.isAttached + " " + !MENUC.IsGaming );
		//gravity
		if ( !isRunning ) rigid.ApplyImpulse( new Vector3( 0, 0, gravity * -1 * .05f ) );
		else rigid.ApplyImpulse( new Vector3( 0, 0, gravity * -1 ) );
		//Sng.LogOnce( "InputActive == " + inputActive );
		//input
		if ( inputActive )
		{

			var dx = (float)Math.Min( 10000f, Math.Abs( Mouse.Delta.x ) * Math.Pow( FC.Set.Sensitivity, 2 ) ) * 1.7f * Math.Sign( Mouse.Delta.x );
			var dy = (float)Math.Min( 10000f, Math.Abs( Mouse.Delta.y ) * Math.Pow( FC.Set.Sensitivity, 2 ) ) * 1.7f * Math.Sign( Mouse.Delta.y );


			if ( FC.Set.MouseInvertX ) dx *= -1f;
			if ( FC.Set.MouseInvertY ) dy *= -1f;

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
				if ( !isRunning ) EngStart();
				else gain += gravity / gainStep;
			}
			else if ( !isRunning ) EngStart( false );

			if ( !Input.Down( "Up" ) && !Input.Down( "Down" ) )
			{
				//gain -= (gain - gravity) / settleStep;
			}
			if ( !isRunning )
			{
				dx = 0f; dy = 0f; dz = 0f; gain = 0;
			}
			if ( gain > maxGain ) gain = maxGain;
			if ( gain < maxGain * -1 ) gain = maxGain * -1;

			Vector3 gainAng = new Vector3( 0, 0, gain ) * Transform.Rotation;
			rigid.ApplyImpulse( gainAng );
			//if horizontal velocity is too high
			//if (new Vector3(rigid.Velocity.x,rigid.Velocity.y, 0).Length > maxSpeed)
			//{
			//if same direction
			//if (gainAng.x * rigid.Velocity.x >0) gainAng *= new Vector3(0, 1, 1);
			//if (gainAng.y * rigid.Velocity.y >0) gainAng *= new Vector3(1, 0, 1);
			//}

			//There is a giant trigger collider box that fix force applied to model 
			rigid.ApplyTorque( new Vector3( dx, dy * invertVert, dz ) * Transform.Rotation );
			//experimental
			//rigid.ApplyForce( new Vector3(rigid.Velocity.x * boost, rigid.Velocity.y * boost, 0 ) * Transform.Rotation);
		}
		rigid.Velocity *= new Vector3( horizontalDumping, horizontalDumping, verticalDumping );

		/*
		deltaZ -= Transform.Position.z;
		if ( deltaZ < 0 ) deltaZ = 0;
		else deltaZ = Math.Abs( deltaZ ) * boost;
		rigid.ApplyImpulse( new Vector3(
			Math.Sign( rigid.Velocity.x ) * deltaZ
			, Math.Sign( rigid.Velocity.y ) * deltaZ
			, 0 ) * Transform.Rotation );
		deltaZ = Transform.Position.z;
		*/
		if ( !isRunning )
		{
			GAMEUI.Gain = ((int)(progress / maxGainGravityScale)).ToString();
		}
		else
		{
			GAMEUI.Gain = ((int)(gain / maxGain * 100f)).ToString();
		}
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
		GameObject.Transform.ClearInterpolation();
		if ( engineRestart )
			EngOff();
		else if ( isRunning ) gain = gravity;
	}
	public void EngOff()
	{
		progress = 0;
		isRunning = false;
		gain = 0f;
	}
	public void EngStart( bool increase = true )
	{
		progress += increase ? 1 : -1;
		if ( progress < 0 ) progress = 0;
		if ( progress >= 100 )
		{
			progress = 100;
			isRunning = true;
			gain = gravity;
		}
	}
}
