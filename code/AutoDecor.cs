namespace Sandbox;

public sealed class AutoDecor : Component
{
	[Property, Description( "For the box zones. If checked, it will be decorated with color lines on the borders" )]
	public bool AutoDecorate { get; set; } = true;

	[Property]
	public Color ColorOfTheLines { get; set; }
}
