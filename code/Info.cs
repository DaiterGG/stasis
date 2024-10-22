namespace Stasis;

public sealed class Info : Component
{
    [Group("General"),
        Property,
        Description("Author of the map")]
    public string Author { get; set; } = "Author Name";

    [Group("General"),
            Property,
            Description("Check out discord for more info")]
    public int DifficultyTier { get; set; } = 1;

    [Group("General"),
        Property,
        Description("Version of the map, keep it in the format of x.y")]
    public string Version { get; set; } = "1.0";

    [Group("Speedrun"),
        Property,
        Description("Additional rewards for fast completion")]
    public bool SpeerunMap { get; set; } = true;

    [Group("Speedrun"),
        Property,
        HideIf(nameof(SpeerunMap), false),
        Description("time in seconds")]
    public float GoldTime { get; set; } = 12.3f;

    [Group("Speedrun"),
        Property,
        HideIf(nameof(SpeerunMap), false),
        Description("time in seconds")]
    public float SilverTime { get; set; } = 15.5f;

    [Group("Speedrun"),
        Property,
        HideIf(nameof(SpeerunMap), false),
        Description("time in seconds")]
    public float BronzeTime { get; set; } = 25.0f;
}

