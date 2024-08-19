namespace Sandbox;
public sealed class DontDestroy : Component
{
	protected override void OnAwake() //you're wellcome
	{
		base.OnAwake();

		GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;
	}
}
