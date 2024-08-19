namespace Sandbox;

public sealed class Info : Component
{
	[Group( "General" ),
		Property,
		Description( "Name of the map" )]
	public string DisplayName { get; set; } = "My Cool Map";

	[Group( "General" ),
		Property,
		DefaultValue( "Author Name" ),
		Description( "Author of the map and personal link (optional)" )]
	public Dictionary<string, string> Author { get; set; }

	[Group( "General" ),
		Property,
		Description( "Version of the map, keep it in the format of x.y" )]
	public string Version { get; set; } = "1.0";

	[Group( "General" ),
		Property,
		TextArea,
		Description( "All the information about the map" )]
	public string Description { get; set; } = "Enjoy my new map \nAnd thanks for playing";

	[Group( "Speedrun" ),
		Property,
		Description( "Additional rewards for fast completion" )]
	public bool SpeerunMap { get; set; } = true;

	[Group( "Speedrun" ),
		Property,
		HideIf( nameof( SpeerunMap ), false ),
		Description( "time in seconds" )]
	public float GoldTime { get; set; } = 12.3f;

	[Group( "Speedrun" ),
		Property,
		HideIf( nameof( SpeerunMap ), false ),
		Description( "time in seconds" )]
	public float SilverTime { get; set; } = 15.5f;

	[Group( "Speedrun" ),
		Property,
		HideIf( nameof( SpeerunMap ), false ),
		Description( "time in seconds" )]
	public float BronzeTime { get; set; } = 25.0f;
}
