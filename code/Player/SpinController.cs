using System;

namespace Sandbox;

public sealed class SpinController : Component
{
    [Property] Rigidbody PropRig;
	[Property, Range(0, 10000f,100f),DefaultValue(1000f)] public float BladeGravity;
    EngineComponent ENGINE;
    GameObject PLAYEROBJ;
	MainTimer TIMER;
    float speedMult = 0.2f;
    float speed;
    public bool isAttached;
	float BladeOffset = 90f;
	List<GameObject> blades = new List<GameObject>();
    protected override void OnAwake()
    {
        ENGINE = Sng.Inst.Player.Engine;
		TIMER = Sng.Inst.Timer;
        PLAYEROBJ = Sng.Inst.Player.GameObject;
		PropRig.GameObject.Children.ToList().ForEach( x => { blades.Add( x ); } );
    }
    protected override void OnStart()
    {
        RestartSpin();
    }
    protected override void OnFixedUpdate()
    {
        if ( !isAttached ) return;
        if ( ENGINE.isRunning )
        {
            PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.gain / ENGINE.gravity * 100 * speedMult, 0 );
        }
        else PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.progress * speedMult, 0 );
    }
    public void SpinCollision()
    {
		if ( !isAttached ) return;
        blades.ForEach( x => {
            x.Parent = PLAYEROBJ;
			x.Transform.Position = PropRig.Transform.Position;
			var rig = x.Components.Get<Rigidbody>();
			rig.ApplyImpulse( new Vector3(0, 800f * (ENGINE.progress + 5f) / 100f, 100f) * x.Transform.Rotation );
			rig.ApplyTorque( new Vector3( 4000f ) );
	        } );
        isAttached = false;
		ENGINE.inputActive = false;
    }
    public void RestartSpin()
    {
        blades.ForEach(x => {
			x.Parent = PropRig.GameObject;
			x.Transform.Position = PropRig.Transform.Position;
			x.Transform.Rotation = PropRig.Transform.Rotation 
				* Rotation.From( 0, x.Components.Get<SpinTrigger>().rotationOffset, 0 );
			var rig = x.Components.Get<Rigidbody>();
			rig.Velocity = 0;
			rig.AngularVelocity = 0;
			} );
        isAttached = true;
    }
}
