using System.Globalization;
using System.Windows.Media;
using MujiScreenSaver.Models;

namespace MujiScreenSaver.Services;

public sealed record PaletteColorToken(string Shade, string Hex)
{
    public string Oklch => OklchColorConverter.FromHex(Hex);
}

public sealed record ThemeColors(
    PaletteColorToken Paper,
    PaletteColorToken Ink,
    PaletteColorToken Muted,
    PaletteColorToken Rule);

public sealed record ColorPaletteDefinition(
    ColorPalette Value,
    string DisplayName,
    ThemeColors Light,
    ThemeColors Dark)
{
    public Brush LightPreviewBrush => CreateBrush(Light.Paper.Hex);

    public Brush DarkPreviewBrush => CreateBrush(Dark.Paper.Hex);

    public ThemeColors For(bool isLight) => isLight ? Light : Dark;

    private static Brush CreateBrush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}

public static class ColorPaletteCatalog
{
    private static readonly IReadOnlyList<ColorPaletteDefinition> Definitions =
    [
        Define(ColorPalette.Orange, "Orange", "#FFF4F0", "#FFE1D8", "#FFC3B0", "#FFA287", "#B13F1C", "#933215", "#3A1307"),
        Define(ColorPalette.Lime, "Lime", "#FBFDEB", "#F3F8C9", "#E5F19A", "#CFE55D", "#647D06", "#4A5A05", "#1B2102"),
        Define(ColorPalette.Yellow, "Yellow", "#FFFBE5", "#FFF9CC", "#FFF699", "#FFF366", "#BFB800", "#8A8400", "#1F1A0B"),
        Define(ColorPalette.Green, "Green", "#EDFFF2", "#D3FCE1", "#A9F5C4", "#7EEE9E", "#239944", "#1A7A37", "#082F13"),
        Define(ColorPalette.Cyan, "Cyan", "#F2FBFC", "#E6F6F8", "#C9EDF1", "#9EDFE6", "#1B7D88", "#145C63", "#061F22"),
        Define(ColorPalette.Blue, "Blue", "#EAF4FF", "#CFE6FE", "#A9D5FE", "#7FBDFD", "#1356A6", "#0D3F7F", "#041729"),
        Define(ColorPalette.Purple, "Purple", "#FAF7FD", "#F1EAF9", "#E0D2F2", "#C7AEE8", "#55388A", "#3E2A66", "#150E26"),
        Define(ColorPalette.Pink, "Pink", "#FFF4F7", "#FDE6EC", "#F9C7D5", "#F2A0B8", "#9B3357", "#742642", "#2A0B16"),
        Define(ColorPalette.Red, "Red", "#FFF4F3", "#FEE5E3", "#FECACA", "#F87171", "#991B1B", "#7F1D1D", "#2A0505"),
        Define(ColorPalette.NeutralSet, "Neutral Set", "#FAFAFA", "#F3F3F3", "#E5E5E5", "#D1D1D1", "#444444", "#2C2C2C", "#0B0B0B")
    ];

    public static IReadOnlyList<ColorPaletteDefinition> All => Definitions;

    public static ColorPaletteDefinition Get(ColorPalette palette) =>
        Definitions.FirstOrDefault(definition => definition.Value == palette) ?? Definitions[0];

    private static ColorPaletteDefinition Define(
        ColorPalette value,
        string name,
        string shade50,
        string shade100,
        string shade200,
        string shade300,
        string shade700,
        string shade800,
        string shade950) =>
        new(
            value,
            name,
            new ThemeColors(new("50", shade50), new("950", shade950), new("700", shade700), new("200", shade200)),
            new ThemeColors(new("950", shade950), new("100", shade100), new("300", shade300), new("800", shade800)));
}

public static class OklchColorConverter
{
    public static string FromHex(string hex)
    {
        var rgb = (Color)ColorConverter.ConvertFromString(hex);
        var red = ToLinear(rgb.R / 255d);
        var green = ToLinear(rgb.G / 255d);
        var blue = ToLinear(rgb.B / 255d);

        var l = CubeRoot(0.4122214708 * red + 0.5363325363 * green + 0.0514459929 * blue);
        var m = CubeRoot(0.2119034982 * red + 0.6806995451 * green + 0.1073969566 * blue);
        var s = CubeRoot(0.0883024619 * red + 0.2817188376 * green + 0.6299787005 * blue);

        var lightness = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s;
        var a = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s;
        var b = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s;
        var chroma = Math.Sqrt(a * a + b * b);
        var hue = Math.Atan2(b, a) * 180d / Math.PI;

        if (hue < 0)
        {
            hue += 360d;
        }

        return string.Create(CultureInfo.InvariantCulture, $"oklch({lightness * 100:F2}% {chroma:F4} {hue:F2})");
    }

    private static double ToLinear(double component) =>
        component <= 0.04045 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);

    private static double CubeRoot(double value) => Math.Pow(value, 1d / 3d);
}
