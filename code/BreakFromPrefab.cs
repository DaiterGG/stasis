namespace Sandbox;

public sealed class BreakFromPrefab : Component
{
    protected override void OnStart()
    {
        GameObject.BreakFromPrefab();
    }
}
