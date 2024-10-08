using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Cryptoquip.Models;
using Cryptoquip.Utility;
using Splat;

namespace Cryptoquip;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RegisterDependencies();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static void RegisterDependencies()
    {
        Locator.CurrentMutable.RegisterConstant<IDecoderRing>(new DecoderRing());
        Locator.CurrentMutable.RegisterConstant(new WordList());
    }
}