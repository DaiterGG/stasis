using System;

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
	protected override void OnStart()
	{
		base.OnStart();
		ResetPos();
	}

	public void OnCollisionStart( Collision col )
	{

		if ( col.Other.GameObject.Tags.Contains( "particle" ) ) return; // not secessary since blades have 'player'
		SPIN.SpinCollision();
	}
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( !SPIN.isAttached )
		{
			rig.ApplyForce( new Vector3( 0, 0, SPIN.BladeGravity * -1 ) );
			return;
		}
		//patch for bug in physics
		if ( Math.Abs( SPIN.PropRig.Transform.Position.y - Transform.Position.y ) > 2f )
		{
			ResetPos();
		}
	}
	private void ResetPos()
	{
		GameObject.Parent = SPIN.PropRig.GameObject;
		rig.Velocity = 0;
		rig.AngularVelocity = 0;

		Transform.Position = SPIN.PropRig.Transform.Position;
		Transform.Rotation = SPIN.PropRig.Transform.Rotation
			* Rotation.From( 0, rotationOffset, 0 );
		Transform.Position += new Vector3( 0, -1f, 0 ) * Transform.Rotation;
		Transform.ClearInterpolation();
	}
}