using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using System;

namespace AipmRegister.Gui;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    //
    // Inter (the default) only carries Latin glyphs. We embed Noto Sans KR
    // and register it as a font fallback so Korean text renders identically
    // on every OS without requiring the user to have CJK fonts installed.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily = new FontFamily(
                            "avares://AipmRegister.Gui/Assets/Fonts/NotoSansKR.ttf#Noto Sans KR"),
                    },
                },
            })
            .LogToTrace();
}
