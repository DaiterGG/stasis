namespace Sandbox;

public sealed class PlayerComp : Component
{
	[Property] public readonly GameObject Body;
	[Property] public readonly SpinController SpinC;
	[Property] public readonly EngineComponent Engine;
	[Property] public readonly CameraControl CameraC;
}

