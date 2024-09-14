using System;
using Stasis.Player;

namespace Stasis.Zones;
public sealed class Beacon : Component, Component.ITriggerListener, IChangeColorZone
{
	[Property, Description( "If two have the same ID, they will count as one" )]
	public int ID { get; set; } = 1;

	[Property, Description( "Change Color On Trigger" )]
	public bool ChangeColor { get; set; } = true;

	[Property, ShowIf( "ChangeColor", true )]
	public Color ColorBefore { get; set; } = new Color( 1f, 0.933f, 0f );

	[Property, ShowIf( "ChangeColor", true )]
	public Color ColorAfter { get; set; } = new Color( 0, 0.62f, 0.024f );
	[Property, Description( "Put any zone here to block it (End zone blocked by default)" )]
	public List<GameObject> BlockUntillActivated { get; set; } = new List<GameObject>();

	public bool IsActivated { get; private set; } = false;

	public int Locked { get; set; } = 0;
	protected override void OnStart()
	{
		IsActivated = false;
		UpdateLocked();
		ColorUpdate();
	}
	public void ActivateToggle( bool activate = true )
	{
		if ( IsActivated == activate ) return;
		IsActivated = activate;
		UpdateLocked();
		ColorUpdate();
	}
	void UpdateLocked()
	{
		foreach ( var zone in BlockUntillActivated )
		{
			var zoneC = zone.Components.Get<IZone>();
			if ( zoneC == null ) continue;
			zoneC.Locked += IsActivated ? 1 : -1;
		}
	}

	public void ColorUpdate()
	{
		if ( !ChangeColor ) return;
		var md = GameObject.Components.Get<ModelRenderer>();
		if ( md != null ) md.Tint = IsActivated ? ColorAfter : ColorBefore;
	}

	public void OnTriggerEnter( Collider col )
	{
		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.IsAttached ) return;
		var eng = col.GameObject.Components.Get<EngineComponent>();
		if ( col is BoxCollider && eng != null ) return;
		if ( Locked != 0 ) Sng.Inst.ZoneC.ZoneBlockedMessage();
		else Sng.Inst.ZoneC.BeaconAcitvate( ID );
	}
	protected override void OnAwake()
	{
		ColorUpdate();
	}
}
