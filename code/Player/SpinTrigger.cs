namespace Sandbox;

public sealed class SpinTrigger : Component, Component.ICollisionListener
{
	[Property, Range( 0, 360f )] public float rotationOffset;
	SpinController SPIN;
	Rigidbody rig;

	protected override void OnAwake()
	{
		SPIN = Sng.Inst.Player.SpinC;
		rig = GameObject.Components.Get<Rigidbody>();
	}
	public void OnCollisionStart( Collision col )
	{

		if ( col.Other.GameObject.Tags.Contains( "particle" ) ) return; // not secessary since blades have 'player'
		SPIN.SpinCollision();
	}
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !SPIN.isAttached ) rig.ApplyForce( new Vector3( 0, 0, SPIN.BladeGravity * -1 ) );
	}
}
