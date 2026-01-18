using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GekkoMusic.Services;
using System.Collections.ObjectModel;

namespace GekkoMusic.ViewModels
{
    public partial class DownloadedViewModel : ObservableObject
    {
        private readonly AudioPlayerService _audio;
        private readonly PlayerViewModel _player;

        public ObservableCollection<DownloadSong> Songs { get; }

        public DownloadedViewModel(
            DownloadStorageService storage,
            AudioPlayerService audio,
            PlayerViewModel player)
        {
            Songs = storage.Downloads;
            _audio = audio;
            _player = player;
        }

        [RelayCommand]
        private void Play(DownloadSong song)
        {
            if (song == null || string.IsNullOrEmpty(song.FilePath))
                return;

            _player.Thumbnail = song.ThumbnailPath;
            _player.Uploader = song.Uploader;
            _player.PlayFile(song.FilePath, song.Title);
        }
    }

    // ======================
    // MODELS
    // ======================

   

    // ======================
    // DIRECTORY HELPERS
    // ======================

    public static class MusicDirectories
    {
        public static string Downloads =>
            Path.Combine(FileSystem.AppDataDirectory, "Downloads");

        public static string TempMusic =>
            Path.Combine(FileSystem.AppDataDirectory, "TempMusic");
    }
}