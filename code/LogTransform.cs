namespace Sandbox;

public sealed class LogTransform : Component
{
	protected override void OnUpdate()
	{

		Log.Info(Transform.Rotation.Pitch() + " " + Transform.Rotation.Yaw() + " " + Transform.Rotation.Roll());
	}
}
