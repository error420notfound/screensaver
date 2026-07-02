using MujiScreenSaver.Modes;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class CommandLineOptionsTests
{
    [Fact]
    public void NoArgumentsOpenConfiguration()
    {
        var options = CommandLineOptions.Parse([]);

        Assert.Equal(ScreenSaverLaunchMode.Configure, options.Mode);
    }

    [Theory]
    [InlineData("/s")]
    [InlineData("-S")]
    public void ScreenSaverArgumentStartsFullScreen(string argument)
    {
        var options = CommandLineOptions.Parse([argument]);

        Assert.Equal(ScreenSaverLaunchMode.FullScreen, options.Mode);
    }

    [Fact]
    public void PreviewAcceptsSeparatedHandle()
    {
        var options = CommandLineOptions.Parse(["/p", "1234"]);

        Assert.Equal(ScreenSaverLaunchMode.Preview, options.Mode);
        Assert.Equal(new IntPtr(1234), options.PreviewHandle);
    }

    [Fact]
    public void PreviewAcceptsColonHandle()
    {
        var options = CommandLineOptions.Parse(["/p:1234"]);

        Assert.Equal(ScreenSaverLaunchMode.Preview, options.Mode);
        Assert.Equal(new IntPtr(1234), options.PreviewHandle);
    }
}
