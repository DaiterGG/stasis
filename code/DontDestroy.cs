using Sandbox;

public sealed class DontDestroy : Component
{
	protected override void OnAwake()
	{
		base.OnAwake();

		GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;
	}
}
