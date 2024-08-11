namespace Sandbox;

public sealed class PlayerSng : Component
{
	public static PlayerSng Player;
	[Property] public readonly GameObject Body;
	[Property] public readonly EngineComponent Engine;
	public GameObject Self { get; private set; }

	protected override void OnAwake()
	{
		Player = this;
		Self = GameObject;
	}
}
