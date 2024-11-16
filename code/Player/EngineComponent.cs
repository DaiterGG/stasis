using Stasis.Data;
using Stasis.UI;

namespace Stasis.Player;
public sealed class EngineComponent : Component
{
    IngameUI GAMEUI;
    MenuController MENUC;
    Timer TIMER;
    Sng SNG;
    FreeCam FREECAM;
    FileControl FC;
    SpinControl SPINC;

    public bool isRunning { get; private set; }
    public int progress { get; private set; }
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
    public bool InputActive
    {
        get
        {
            return !FREECAM.GameObject.Enabled && SPINC.IsAttached && SNG.GameState == GameState.Play;
        }
        set { }
    }

    //exp
    //[Property, Range(1f, 300f, 1f), DefaultValue(1f)] public readonly float boost = 1.1f;
    //float deltaZ = 0;
    //


    public void OnAwakeInit()
    {
        SNG = Sng.Inst;
        FREECAM = SNG.Player.CameraC.FreeCam;
        SPINC = SNG.Player.SpinC;
        MENUC = SNG.MenuC;
        GAMEUI = MENUC.IngameUI;
        FC = SNG.FileC;
        TIMER = SNG.Timer;
        rigid = GameObject.Components.Get<Rigidbody>();
        ResetPos( true );
    }
    public void OnFixedGlobal()
    {
        if ( SNG.GameState != GameState.Play ) return;

        //gravity
        // if (!isRunning) rigid.ApplyImpulse(new Vector3(0, 0, gravity * -1 * .05f));
        if ( !isRunning ) rigid.ApplyImpulse( new Vector3( 0, 0, 0 ) );
        else rigid.ApplyImpulse( new Vector3( 0, 0, gravity * -1 ) );

        //input
        if ( InputActive )
        {
            var dx = (float)Math.Min( 10000f, Math.Abs( Mouse.Delta.x / 1 ) * Math.Pow( FC.Set.Sensitivity, 2 ) ) * 1.7f * Math.Sign( Mouse.Delta.x / 1 );
            var dy = (float)Math.Min( 10000f, Math.Abs( Mouse.Delta.y / 1 ) * Math.Pow( FC.Set.Sensitivity, 2 ) ) * 1.7f * Math.Sign( Mouse.Delta.y / 1 );
            //dy = dy / 30f;
            //dx = dx / 30f;

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
                if ( !isRunning ) EngStartProcess();
                else gain += gravity / gainStep;
            }
            else if ( !isRunning ) EngStartProcess( false );

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

            Vector3 gainAng = new Vector3( 0, 0, gain ) * WorldRotation;
            rigid.ApplyImpulse( gainAng );
            //if horizontal velocity is too high
            //if (new Vector3(rigid.Velocity.x,rigid.Velocity.y, 0).Length > maxSpeed)
            //{
            //if same direction
            //if (gainAng.x * rigid.Velocity.x >0) gainAng *= new Vector3(0, 1, 1);
            //if (gainAng.y * rigid.Velocity.y >0) gainAng *= new Vector3(1, 0, 1);
            //}

            //There is a giant trigger collider box that fix force applied to model 
            rigid.ApplyTorque( new Vector3( dx, dy * -1, dz ) * WorldRotation );
            //experimental
            //rigid.ApplyForce( new Vector3(rigid.Velocity.x * boost, rigid.Velocity.y * boost, 0 ) * WorldRotation);
        }
        rigid.Velocity *= new Vector3( horizontalDumping, horizontalDumping, verticalDumping );

        /*
        deltaZ -= WorldPosition.z;
        if ( deltaZ < 0 ) deltaZ = 0;
        else deltaZ = Math.Abs( deltaZ ) * boost;
        rigid.ApplyImpulse( new Vector3(
            Math.Sign( rigid.Velocity.x ) * deltaZ
            , Math.Sign( rigid.Velocity.y ) * deltaZ
            , 0 ) * WorldRotation );
        deltaZ = WorldPosition.z;
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


    public void ResetPos( bool engineRestart )
    {
        GameObject.WorldPosition = GameObject.Parent.WorldPosition + new Vector3( 0, 0, bodyOffsetZ );
        GameObject.WorldRotation = GameObject.Parent.WorldRotation;
        rigid.Velocity = new Vector3( 0 );
        rigid.AngularVelocity = new Vector3( 0 );
        GameObject.Transform.ClearInterpolation();
        if ( engineRestart )
            EngOn( false );
        else if ( isRunning ) gain = gravity;
    }
    public void EngOn( bool on )
    {
        progress = on ? 100 : 0;
        isRunning = on;
        TIMER.Update();
        gain = on ? gravity : 0f;
    }
    void EngStartProcess( bool increaseProgress = true )
    {
        progress += increaseProgress ? 1 : -1;
        if ( progress < 0 ) progress = 0;
        if ( progress >= 100 )
        {
            EngOn( true );
        }
    }
    public void ApplySaveState( SaveState state )
    {
        WorldPosition = state.Transform;
        WorldRotation = Rotation.From(
            state.Rotation.Pitch(),
            state.Rotation.Yaw(),
            state.Rotation.Roll()
        );

        rigid.Velocity = state.Velocity;
        rigid.AngularVelocity = state.AngularVelocity;
        progress = state.Progress;
        gain = state.Gain;
        isRunning = state.EngineRunning;
        Transform.ClearInterpolation();
    }
    public void ApplyTick( Vector3 transform, Rotation rotation )
    {
        WorldPosition = transform;
        WorldRotation = rotation;
    }
}
