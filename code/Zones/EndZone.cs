using System;

namespace Stasis.Zones;
public sealed class EndZone : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider col )
	{
		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.isAttached ) return;
		if ( col is BoxCollider ) return;
		Sng.Inst.EndZoneEnter( GameObject, col );
	}
}
