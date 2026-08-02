using System.Text.Json;
using System.Text.RegularExpressions;

namespace TabSale.Web.Services;

public sealed class ThemeConfigStore
{
    private static readonly HashSet<string> Themes = ["classic", "winter", "market", "contrast", "custom"];
    private static readonly Regex ColorPattern = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);
    private readonly string path;
    private readonly object gate = new();
    private ThemeConfig current;

    public ThemeConfigStore(string path)
    {
        this.path = path;
        current = Load();
    }

    public ThemeConfig Current { get { lock (gate) return current with { }; } }

    public void Save(ThemeConfig config)
    {
        var validated = Validate(config);
        lock (gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(validated, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryPath, path, true);
            current = validated;
        }
    }

    private ThemeConfig Load()
    {
        try
        {
            if (File.Exists(path)) return Validate(JsonSerializer.Deserialize<ThemeConfig>(File.ReadAllText(path)) ?? new());
        }
        catch { /* Invalid configuration falls back to safe defaults. */ }
        return new();
    }

    private static ThemeConfig Validate(ThemeConfig config) => new()
    {
        Theme = Themes.Contains(config.Theme) ? config.Theme : "classic",
        Ink = ValidColor(config.Ink, "#102a2a"),
        Brand = ValidColor(config.Brand, "#167d6d"),
        Lime = ValidColor(config.Lime, "#e4f26b"),
        Paper = ValidColor(config.Paper, "#f4f6f1"),
        Danger = ValidColor(config.Danger, "#bc3c3c")
    };

    private static string ValidColor(string? value, string fallback) => value is not null && ColorPattern.IsMatch(value) ? value : fallback;
}

public sealed record ThemeConfig
{
    public string Theme { get; init; } = "classic";
    public string Ink { get; init; } = "#102a2a";
    public string Brand { get; init; } = "#167d6d";
    public string Lime { get; init; } = "#e4f26b";
    public string Paper { get; init; } = "#f4f6f1";
    public string Danger { get; init; } = "#bc3c3c";
}
