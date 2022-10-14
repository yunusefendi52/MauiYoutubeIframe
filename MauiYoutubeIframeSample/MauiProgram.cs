using MauiYoutubeIframe;

namespace MauiYoutubeIframeSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(static h =>
            {
                h.AddHandler<YoutubeIframe, YoutubeIframeHandler>();
            });

        return builder.Build();
    }
}

