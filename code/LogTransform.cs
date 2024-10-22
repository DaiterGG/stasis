namespace Sandbox;

public sealed class LogTransform : Component
{
    protected override void OnUpdate()
    {

        Log.Info(WorldRotation.Pitch() + " " + WorldRotation.Yaw() + " " + WorldRotation.Roll());
    }
}
