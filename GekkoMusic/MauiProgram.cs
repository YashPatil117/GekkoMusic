using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using GekkoMusic.Services;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using GekkoMusic.ViewModels;
using GekkoMusic.Cards;
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
            builder.ConfigureMauiHandlers(handlers => { });

            builder.Services.AddSingleton(AudioManager.Current);

            builder.Services.AddSingleton<AudioPlayerService>();

            builder.Services.AddSingleton<YoutubeDlpService>();

            builder.Services.AddSingleton<PlaylistStorageService>();

            builder.Services.AddSingleton<PlayerViewModel>();

            builder.Services.AddTransient<MainPage>();

            builder.Services.AddSingleton<LikedSongStorageService>();

            builder.Services.AddSingleton<LikedSongsViewModel>();

            builder.Services.AddTransient<Cards.LikedSongs>();

            builder.Services.AddSingleton<DownloadStorageService>();

            //builder.Services.AddSingleton<DownloadStorageService>();

            builder.Services.AddSingleton<DownloadedViewModel>();

            builder.Services.AddTransient<Cards.Downloaded>();
            
            


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
