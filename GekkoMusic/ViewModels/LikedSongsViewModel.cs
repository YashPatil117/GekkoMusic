using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GekkoMusic.Services;
using System.Collections.ObjectModel;

namespace GekkoMusic.ViewModels
{
    public partial class LikedSongsViewModel : ObservableObject
    {
        private readonly LikedSongStorageService _storage;

        // ✅ UI-bound collection
        [ObservableProperty]
        private ObservableCollection<YoutubeVideo> likedSongs = new();

        public LikedSongsViewModel(LikedSongStorageService storage)
        {
            _storage = storage;
            LoadAsync();
        }

        // ✅ Load once from storage
        private async void LoadAsync()
        {
            var songs = await _storage.LoadAsync();

            LikedSongs.Clear();
            foreach (var song in songs)
                LikedSongs.Add(song);
        }

        // ✅ ONE command – UI + storage synced
        [RelayCommand]
        private async Task Unlike(YoutubeVideo video)
        {
            if (video == null)
                return;

            // 1. remove from UI
            LikedSongs.Remove(video);

            // 2. remove from persistent storage
            await _storage.RemoveAsync(video);
        }
    }
}
