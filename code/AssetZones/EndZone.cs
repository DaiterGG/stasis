using Sandbox;

public sealed class EndZone : Component, Component.ITriggerListener
{
	Sng SNG = Sng.Inst;
	protected override void OnEnabled()
	{
		base.OnEnabled();
	}
	public void OnTriggerEnter(Collider col )
	{
		SNG.EndZoneEnter(GameObject, col);
		
	}
}
