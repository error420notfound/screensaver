using System.Globalization;

namespace MujiScreenSaver.Modes;

public enum ScreenSaverLaunchMode
{
    Configure,
    FullScreen,
    Preview
}

public sealed class CommandLineOptions
{
    private CommandLineOptions(ScreenSaverLaunchMode mode, IntPtr? previewHandle)
    {
        Mode = mode;
        PreviewHandle = previewHandle;
    }

    public ScreenSaverLaunchMode Mode { get; }

    public IntPtr? PreviewHandle { get; }

    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandLineOptions(ScreenSaverLaunchMode.Configure, null);
        }

        var first = args[0].Trim();
        var normalized = first.Replace('-', '/').ToLowerInvariant();

        if (normalized == "/s")
        {
            return new CommandLineOptions(ScreenSaverLaunchMode.FullScreen, null);
        }

        if (normalized == "/c" || normalized.StartsWith("/c:", StringComparison.Ordinal))
        {
            return new CommandLineOptions(ScreenSaverLaunchMode.Configure, null);
        }

        if (normalized == "/p" || normalized.StartsWith("/p:", StringComparison.Ordinal))
        {
            var handleText = normalized.StartsWith("/p:", StringComparison.Ordinal)
                ? first[3..]
                : args.Length > 1 ? args[1] : string.Empty;

            if (TryParseHandle(handleText, out var handle) && handle != IntPtr.Zero)
            {
                return new CommandLineOptions(ScreenSaverLaunchMode.Preview, handle);
            }

            return new CommandLineOptions(ScreenSaverLaunchMode.Configure, null);
        }

        return new CommandLineOptions(ScreenSaverLaunchMode.Configure, null);
    }

    private static bool TryParseHandle(string text, out IntPtr handle)
    {
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (long.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
            {
                handle = new IntPtr(hexValue);
                return true;
            }
        }

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            handle = new IntPtr(value);
            return true;
        }

        handle = IntPtr.Zero;
        return false;
    }
}
