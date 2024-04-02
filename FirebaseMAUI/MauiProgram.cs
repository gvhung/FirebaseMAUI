using Microsoft.Extensions.Logging;

namespace FirebaseMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp(Func<MauiAppBuilder, MauiAppBuilder> configurePlatformServices = null)
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();
            builder.ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

            builder.ConfigureServices();
            builder.ConfigureViews();
            builder.ConfigureViewModels();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            configurePlatformServices?.Invoke(builder);

            return builder.Build();
        }


        static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
        {

#if ANDROID
            builder.Services.AddSingleton<INotifyService, FirebaseMAUI.Droid.NotifyService>();
#elif IOS
            builder.Services.AddSingleton<INotifyService, FirebaseMAUI.iOS.NotifyService>();
#elif MACCATALYST
            builder.Services.AddSingleton<INotifyService, FirebaseMAUI.MacCatalyst.NotifyService>();
#elif WINDOWS
            builder.Services.AddSingleton<INotifyService, FirebaseMAUI.WinUI.NotifyService>();
#endif

            return builder;
        }

        static MauiAppBuilder ConfigureViews(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<MainPage>();

            return builder;
        }

        static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddTransient<MainPageViewModel>();

            return builder;
        }
    }
}
