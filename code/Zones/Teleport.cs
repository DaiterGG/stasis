
using Stasis.Player;

namespace Stasis.Zones;

public sealed class Teleport : Component, Component.ITriggerListener, IZone
{
	public int ID {get;set;}
	public int Locked {get; set;} = 0;
	[Property]
	public bool ResetRotation = true;
	[Property]
	public bool ResetVelocity = true;
	[Property]
	public bool RepairPropeller = true;
	[Property, Description("Could be any game object")]
	public GameObject Exit;
	protected override void OnStart()
	{
		base.OnStart();
		if(Exit == null){
			Log.Warning("Teleport with no spawn");
			Exit = GameObject;
		}
	}
	public void OnTriggerEnter( Collider col ){

		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.IsAttached ) return;
		var eng = col.GameObject.Components.Get<EngineComponent>();
		if ( col is BoxCollider && eng != null) return;
		if ( Locked != 0 ) Sng.Inst.ZoneC.ZoneBlockedMessage();
		else Sng.Inst.ZoneC.TeleportPlayer( Exit.Transform, ResetVelocity,ResetRotation,RepairPropeller);
	}
}
