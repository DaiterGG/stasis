namespace Stasis.Player;

public sealed class ProperllerHitbox : Component, Component.ICollisionListener
{
	SpinControl SPIN;
protected override void OnStart()
	{
		base.OnStart();
		SPIN = Sng.Inst.Player.SpinC;
	}

	public void OnCollisionStart( Collision col )
	{
		if ( !SPIN.IsAttached ) return;
		if ( col.Other.GameObject.Tags.Contains( "particle" ) ) return; // not secessary since blades have 'player' tag
		SPIN.SpinCollision();
	}
}
