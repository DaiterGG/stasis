using System;

namespace Stasis.Zones;
public sealed class Beacon : Component, Component.ITriggerListener, IChangeColorZone
{
	[Property, Description("If two have the same ID, they will count as one")]
	public int ID {get; set;} = 1;

	[Property, Description("Change Color On Trigger")]
	public bool ChangeColor {get; set;} = true;

	[Property, ShowIf("ChangeColor", true)]
	public Color ColorBefore {get; set;} = new Color(1f, 0.933f, 0f );

	[Property, ShowIf("ChangeColor", true)]
	public Color ColorAfter {get; set;} = new Color(0, 0.62f, 0.024f);

	public bool IsActivated { get; private set; } = false;

	public void Activate(bool activate = true)
	{
		IsActivated = activate;
		ColorUpdate();
	}

	public void ColorUpdate()
	{
		if ( !ChangeColor ) return;
		var md = GameObject.Components.Get<ModelRenderer>();
		if(md != null) md.Tint = IsActivated ? ColorAfter : ColorBefore;
	}

	public void OnTriggerEnter( Collider col )
	{
		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.isAttached ) return;
		if ( col is BoxCollider ) return;
		Sng.Inst.ZoneC.BeaconAcitvate( ID );
	}
	protected override void OnAwake(){
		ColorUpdate();
	}
}
