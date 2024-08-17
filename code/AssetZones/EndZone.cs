using Sandbox;

public sealed class EndZone : Component, Component.ITriggerListener
{
	public void OnTriggerEnter(Collider col )
	{
		Sng.Inst.EndZoneEnter(GameObject, col);
		
	}
}
