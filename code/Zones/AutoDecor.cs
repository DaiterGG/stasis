namespace Stasis.Zones;

public sealed class AutoDecor : Component
{
	[Property, Description( "Color of the lines" )]
	public Color ColorOfTheLines { get; set; } = new Color(1,0,0);
	[Property, Description( "Width is very small, but increases with distance" )]
	public bool WireFrame{ get; set; } = false;
	[Property, HideIf(nameof(WireFrame), true), Description( "Width of the lines" )]
	public float LineWidth { get; set; } = 3f;
}
