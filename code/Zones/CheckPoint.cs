using Stasis.Player;
namespace Stasis.Zones;

public sealed class CheckPoint : Component, Component.ITriggerListener, IZone
{
	public int ID {get; set;} = 1;
	public int Locked {get; set;} = 0;
	[Property, Description("Could be any game object")]
	public GameObject Spawn;
	protected override void OnStart()
	{
		base.OnStart();
		if(Spawn == null){
			Log.Warning("CheckPoint with no spawn");
			Spawn = GameObject;
		}
	}
	public void OnTriggerEnter( Collider col )
	{
		if ( col.GameObject.Tags.Contains( "particle" ) &&
			!Sng.Inst.Player.SpinC.IsAttached ) return;
		var eng = col.GameObject.Components.Get<EngineComponent>();
		if ( col is BoxCollider && eng != null) return;
		if ( Locked != 0 ) Sng.Inst.ZoneC.ZoneBlockedMessage();
		Sng.Inst.ZoneC.CheckPointActivate(ID);
	}
}
