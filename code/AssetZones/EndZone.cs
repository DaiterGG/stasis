using Sandbox;

public sealed class EndZone : Component, Component.ITriggerListener
{
	protected override void OnEnabled()
	{
		base.OnEnabled();

	}
	public void OnTriggerEnter(Collider col )
	{
		Sng.Inst.EndZoneEnter(GameObject, col);
	}
}
