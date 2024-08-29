using System;

namespace Sandbox;

public sealed class SpinTrigger : Component, Component.ICollisionListener
{
	[Property, Range( 0, 360f )] public float rotationOffset;
	SpinControl SPIN;
	Rigidbody rig;

	protected override void OnStart()
	{
		base.OnStart();
		rig = GameObject.Components.Get<Rigidbody>();
		SPIN = Sng.Inst.Player.SpinC;
		ResetPos();
	}

	public void OnCollisionStart( Collision col )
	{
		if ( !SPIN.isAttached ) return;
		if ( col.Other.GameObject.Tags.Contains( "particle" ) ) return; // not secessary since blades have 'player' tag
		SPIN.SpinCollision();
	}
	public void OnFixedGlobal()
	{
		if ( !SPIN.isAttached )
		{
			rig.ApplyForce( new Vector3( 0, 0, SPIN.BladeGravity * -1 ) );
			return;
		}
		//patch for bug in physics
		if ( Math.Abs( Transform.LocalPosition.y ) > 4f
			|| Math.Abs( Transform.LocalRotation.Pitch() ) > 1f
			|| Math.Abs( Transform.LocalRotation.Roll() ) > 1f )
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
