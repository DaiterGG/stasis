using Stasis.UI;

namespace Stasis.Player;
public sealed class FreeCam : Component
{
    readonly float FORCE = 10f;
    static readonly float BASE_SPEED_MULT = 1f;
    static readonly float SHIFT_SPEED_MULT = 5f;
    static readonly float MOUSE_SENSITIVITY = 3f;
    float mult = BASE_SPEED_MULT;
    Timer TIMER;
    PlayerComp PLAYER;
    MenuController MENUC;
    CameraControl CAMERAC;
    GameObject THIRD;
    Vector3 baseThirdPos;
    public void OnAwakeInit()
    {
        PLAYER = Sng.Inst.Player;
        MENUC = Sng.Inst.MenuC;
        TIMER = Sng.Inst.Timer;
        CAMERAC = PLAYER.CameraC;

        // NOTE: this is bad stuff
        // TODO: fix this
        THIRD = CAMERAC.cameras[0];

        baseThirdPos = THIRD.LocalPosition * new Vector3( 0.5f, 0.5f, 0.5f );
    }
    protected override void OnEnabled()
    {
        TIMER.StopTimer();
        WorldPosition = (baseThirdPos * PLAYER.Body.WorldRotation) + PLAYER.Body.WorldPosition;
        WorldRotation = THIRD.WorldRotation;
        Transform.ClearInterpolation();
    }
    protected override void OnUpdate()
    {
        if ( !GameObject.Enabled ) return;
        if ( Input.Down( "Up" ) )
        { //W or forward        
            WorldPosition += new Vector3( FORCE * mult, 0, 0 ) * WorldRotation;
        }
        if ( Input.Down( "Down" ) )
        {
            WorldPosition += new Vector3( FORCE * mult * -1, 0, 0 ) * WorldRotation;
        }
        if ( Input.Down( "Left" ) )
        {
            WorldPosition += new Vector3( 0, FORCE * mult, 0 ) * WorldRotation;
        }
        if ( Input.Down( "Right" ) )
        {
            WorldPosition += new Vector3( 0, FORCE * mult * -1, 0 ) * WorldRotation;
        }
        if ( Input.Down( "SelfDestruct" ) )
        {
            WorldPosition += new Vector3( 0, 0, FORCE * mult );
        }
        if ( Input.Down( "Crouch" ) )
        {
            WorldPosition += new Vector3( 0, 0, FORCE * mult * -1 );
        }
        if ( Input.Down( "Attack2" ) )
        {
            CAMERAC.UpdatePosition( WorldPosition, WorldRotation );

        }
        if ( Input.Down( "Sprint" ) )
        {
            mult = SHIFT_SPEED_MULT;
        }
        else
        {
            mult = BASE_SPEED_MULT;
        }
        var ee = WorldRotation.Angles();
        ee += Input.AnalogLook * Time.Delta * MOUSE_SENSITIVITY;
        if ( Math.Abs( ee.pitch ) > 90 ) ee.pitch = 90 * Math.Sign( ee.pitch );
        ee.roll = 0;

        WorldRotation = ee.ToRotation();
        Transform.ClearInterpolation();
    }
}
