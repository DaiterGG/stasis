using System;

namespace Stasis.Player;

public sealed class SpinTrigger : Component, Component.ICollisionListener
{
	[Property, Range( 0, 360f )] public float rotationOffset;
	SpinControl SPIN;
	public Rigidbody rig;

	public void OnAwakeInit(){
		rig = GameObject.Components.Get<Rigidbody>();
		SPIN = Sng.Inst.Player.SpinC;
		ResetPos();

	}

	public void OnCollisionStart( Collision col )
	{
		if ( !SPIN.IsAttached ) return;
		if ( col.Other.GameObject.Tags.Contains( "particle" ) ) return; // not secessary since blades have 'player' tag
		SPIN.SpinCollision();
	}
	public void OnFixedGlobal()
	{

		if ( !SPIN.IsAttached )
		{
			rig.ApplyForce( new Vector3( 0, 0, SPIN.BladeGravity * -1 ) );
			return;
		} else if ( Math.Abs( Transform.LocalPosition.y ) > 4f
			|| Math.Abs( Transform.LocalRotation.Pitch() ) > 1f
			|| Math.Abs( Transform.LocalRotation.Roll() ) > 1f 
			|| Transform.LocalPosition.z != 0)
		{
			ResetPos();
		}
	}
	public void ResetPos()
	{
		GameObject.Parent = SPIN.PropRig.GameObject;

		Transform.Position = SPIN.PropRig.Transform.Position;
		Transform.Rotation = SPIN.PropRig.Transform.Rotation
			* Rotation.From( 0, rotationOffset, 0 );
		Transform.Position += new Vector3( 0, -1f, 0 ) * Transform.Rotation;
		Transform.ClearInterpolation();
		rig.Velocity = 0;
		rig.AngularVelocity = 0;
	}
}
