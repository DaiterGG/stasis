namespace Stasis.Zones;

public sealed class AutoDecor : Component
{
	[Property, Description( "For the box zones. If checked, it will be decorated with color lines on the edges" )]
	public bool AutoDecorate { get; set; } = true;

	[Property, HideIf(nameof(AutoDecorate), false), Description( "Color of the lines" )]
	public Color ColorOfTheLines { get; set; } = new Color(1,0,0);
	[Property, HideIf(nameof(AutoDecorate), false), Description( "Width of the lines" )]
	public float LineWidth { get; set; } = 4.5f;
}
