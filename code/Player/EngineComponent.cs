using System;


namespace Sandbox;
public sealed class EngineComponent : Component
{
	IngameUI GAMEUI;
	MenuController MENUC;
	GameObject FREECAM;
	SpinController SPINC;

	public bool inputActive { get; private set; }
	public bool isRunning;
	public int progress;
	[Property] Rigidbody rigid;
	[Property, Range( 0.9f, 1f, 0.001f ), DefaultValue( 0.985f )] public float horizontalDumping;
	[Property, Range( 0, 500f ), DefaultValue( 0f )] public float bodyOffsetZ;
	[Property, Range( 0, 20000f ), DefaultValue( 16000f )] public float turnSpeed;
	[Property, Range( 0, 20000f ), DefaultValue( 40f )] public float gravity;
	[Property, Range( 0, 2000f ), DefaultValue( 100f )] public float gainStep;
	[Property, Range( 0, 500f ), DefaultValue( 100f )] public float settleStep;
	[Property] public Transform turnVector;
	//[Property, Range( 0, 50000f ), DefaultValue( 1000f )] readonly float maxSpeed;
	public float gain { get; private set; } = 0f;
	float maxGainGravityScale = 1.06f;
	public float maxGain { get { return gravity * maxGainGravityScale; } }
	int invertVert = -1;
	//exp
	[Property, Range( 1f, 300f, 1f ), DefaultValue( 1f )] readonly float boost = 1.1f;
	float deltaZ = 0;
	protected override void OnAwake()
	{
		base.OnAwake();
		FREECAM = Sng.Inst.Player.CameraC.FreeCam;
		SPINC = Sng.Inst.Player.SpinC;
		MENUC = Sng.Inst.MenuC;
		GAMEUI = MENUC.IngameUI;
	}
	protected override void OnStart()
	{
		base.OnStart();
		ResetPos( true );
	}
	public void UpdateInput()
	{
		//Log.Info( FREECAM.Enabled + " " + !SPINC.isAttached + " " + !MENUC.IsGaming );
		if ( FREECAM.Enabled || !SPINC.isAttached || !MENUC.IsGaming )
		{
			inputActive = false;
		}
		else
			inputActive = true;
	}
	protected override void OnFixedUpdate()
	{
		//gravity
		if ( !isRunning ) rigid.ApplyImpulse( new Vector3( 0, 0, gravity * -1 * .05f ) );
		else rigid.ApplyImpulse( new Vector3( 0, 0, gravity * -1 ) );

		rigid.Velocity *= horizontalDumping;

		//input
		if ( inputActive )
		{


			var dx = Mouse.Delta.x == 0 ? 0 : Mouse.Delta.x / Math.Abs( Mouse.Delta.x ) * 17000f;
			var dy = Mouse.Delta.y == 0 ? 0 : Mouse.Delta.y / Math.Abs( Mouse.Delta.y ) * 17000f;
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
			else if ( !isRunning ) engStart( false );

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
		{
			engOff();
		}
		else if ( isRunning ) gain = gravity;
	}
	public void engOff()
	{
		progress = 0;
		isRunning = false;
		gain = 0f;
	}
	public void engStart( bool increase = true )
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
