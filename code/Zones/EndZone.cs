using System;

namespace Stasis.Zones;
public sealed class EndZone : Component, Component.ITriggerListener, IZone
{	
	public int ID {get; set;} = 1;
	public void OnTriggerEnter( Collider col )
	{
		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.isAttached ) return;
		if ( col is BoxCollider ) return;
		Sng.Inst.ZoneC.EndZoneEnter( GameObject, col );
	}
}
