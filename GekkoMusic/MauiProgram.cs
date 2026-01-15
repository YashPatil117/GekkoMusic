using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using GekkoMusic.Services;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using GekkoMusic.ViewModels;
namespace GekkoMusic
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            

            var builder = MauiApp.CreateBuilder();
          
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                
                
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton(AudioManager.Current);

            builder.Services.AddSingleton<AudioPlayerService>();

            builder.Services.AddSingleton<YoutubeDlpService>();

            builder.Services.AddSingleton<PlaylistStorageService>();

            builder.Services.AddTransient<PlayerViewModel>();

            builder.Services.AddTransient<MainPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
