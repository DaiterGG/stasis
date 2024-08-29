namespace Sandbox;

public sealed class BreakFromPrefab : Component
{
	protected override void OnAwake()
	{
		GameObject.BreakFromPrefab();
	}
}
