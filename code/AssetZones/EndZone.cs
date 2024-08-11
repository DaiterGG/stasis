using Sandbox;

public sealed class EndZone : Component, Component.ITriggerListener
{
	private Sng _sng;
	protected override void OnAwake()
	{
		base.OnAwake();
		_sng = Sng.Inst;
	}
	public void OnTriggerEnter(Collider col )
	{
		_sng.EndZoneEnter(GameObject, col);
	}
}
