namespace Sandbox.Player;

public sealed class PlayerComp : Component
{
	[Property] public readonly GameObject Body;
	[Property] public readonly SpinControl SpinC;
	[Property] public readonly EngineComponent Engine;
	[Property] public readonly CameraControl CameraC;
	[Property] public readonly SoundControl SoundC;
}

