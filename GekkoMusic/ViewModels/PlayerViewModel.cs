using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GekkoMusic.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GekkoMusic.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly AudioPlayerService _audioService;
        private readonly IDispatcherTimer _timer;
        private readonly PlaylistStorageService _storage;
        private readonly YoutubeDlpService _yt;
        private readonly DownloadStorageService _downloadStorage;
        private readonly LikedSongStorageService _likedStorage;

        public ObservableCollection<Playlist> Playlists => _storage.Playlists;
        public ObservableCollection<YoutubeVideo> SearchResults { get; } = new();
        public bool HasPlaylists => Playlists.Any();

        public PlayerViewModel(
            AudioPlayerService audioService,
            YoutubeDlpService yt,
            PlaylistStorageService storage,
            LikedSongStorageService likedStorage,
            DownloadStorageService downloadStorage)
        {
            _audioService = audioService;
            _yt = yt;
            _downloadStorage = downloadStorage;
            _likedStorage = likedStorage;
            _storage = storage;

            _timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(400);
            _timer.Tick += (_, _) => UpdatePosition();

            Volume = 0.7;

            Playlists.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPlaylists));
        }

        [ObservableProperty]
        private double position;

        [ObservableProperty]
        private double totalDuration;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string? currentFilePath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PlayPauseIcon))]
        private bool isPlaying;

        [ObservableProperty]
        private bool isDragging;

        [ObservableProperty]
        private double volume;

        [ObservableProperty]
        private string songName = string.Empty;

        [ObservableProperty]
        private string thumbnail;

        [ObservableProperty]
        private string uploader;

        public string CurrentTime => FormatTime(Position);
        public string TotalTime => FormatTime(TotalDuration);
        public string PlayPauseIcon => IsPlaying ? "pause_player.png" : "play_player.png";

        [RelayCommand]
        private void PlayPause()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
                return;

            if (_audioService.IsPlaying)
            {
                _audioService.Pause();
                IsPlaying = false;
            }
            else
            {
                _audioService.Resume();
                IsPlaying = true;
            }
        }

        [RelayCommand]
        private void SeekStarted() => IsDragging = true;

        [RelayCommand]
        private void SeekCompleted()
        {
            _audioService.Seek(Position);
            IsDragging = false;
        }

        private void UpdatePosition()
        {
            if (_audioService.Duration > 0 && TotalDuration == 0)
                TotalDuration = _audioService.Duration;

            if (IsDragging || !_audioService.IsPlaying)
                return;

            Position = _audioService.Position;
        }

        partial void OnPositionChanged(double value)
            => OnPropertyChanged(nameof(CurrentTime));

        partial void OnTotalDurationChanged(double value)
            => OnPropertyChanged(nameof(TotalTime));

        partial void OnVolumeChanged(double value)
            => _audioService.Volume = value;

        private static string FormatTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{ts.Minutes}:{ts.Seconds:D2}";
        }

        [RelayCommand]
        private async Task Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return;

            SearchResults.Clear();

            await foreach (var video in _yt.SearchStreamAsync(query, 5))
            {
                SearchResults.Add(video);

                if (SearchResults.Count == 5)
                    break;
            }
        }

        [RelayCommand]
        private async Task ChangePlaylistCover(Playlist playlist)
        {
            if (playlist == null)
                return;

            var path = await PlaylistImageService.PickAndSaveImageAsync(playlist.Id);
            if (path == null)
                return;

            playlist.CoverImagePath = path;
            await _storage.SaveAsync();
        }

        [RelayCommand]
        private async Task AddToPlaylist(YoutubeVideo video)
        {
            var playlists = _storage.Playlists;
            var playlistNames = playlists.Select(p => p.Name).ToArray();

            var selected = await Shell.Current.DisplayActionSheet(
                "Add to Playlist",
                "Cancel",
                null,
                playlistNames
            );

            if (string.IsNullOrEmpty(selected) || selected == "Cancel")
                return;

            var playlist = playlists.First(p => p.Name == selected);

            if (!playlist.Videos.Any(v => v.Id == video.Id))
            {
                playlist.Videos.Add(video);
                await _storage.SaveAsync();
            }
        }

        [RelayCommand]
        private async Task CreatePlaylist()
        {
            var name = await Shell.Current.DisplayPromptAsync(
                "Create Playlist",
                "Enter playlist name");

            if (string.IsNullOrWhiteSpace(name))
                return;

            var playlist = new Playlist
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Videos = new List<YoutubeVideo>()
            };

            bool addCover = await Shell.Current.DisplayAlert(
                "Playlist Cover",
                "Do you want to add a cover image?",
                "Yes",
                "No");

            if (addCover)
            {
                playlist.CoverImagePath =
                    await PlaylistImageService.PickAndSaveImageAsync(playlist.Id);
            }

            Playlists.Insert(0, playlist);
            await _storage.SaveAsync();
        }

        [RelayCommand]
        private async Task DeletePlaylistAsync(Playlist playlist)
        {
            if (playlist == null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Playlist",
                $"Are you sure you want to delete \"{playlist.Name}\"?",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            Playlists.Remove(playlist);
            await _storage.SaveAsync();
        }

        [RelayCommand]
        private async Task LikeSong(YoutubeVideo video)
        {
            if (video == null)
                return;

            await _likedStorage.AddAsync(video);
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlert(
                    "Liked",
                    "Added to liked songs",
                    "OK"));
        }

        [RelayCommand]
        private async Task PlayOnline(YoutubeVideo video)
        {
            if (video == null)
                return;

            // STOP FIRST to unlock files
            _audioService.Stop();
            IsPlaying = false;
            _timer.Stop();

            try
            {
                await Task.Delay(100); // Let cleanup finish

                var tempFile = await _yt.DownloadTempAsync(video.Url);

                CurrentFilePath = tempFile;
                SongName = video.Title;
                Thumbnail = video.ThumbnailUrl;
                Uploader = video.Uploader;
                Position = 0;
                TotalDuration = 0;

                _audioService.PlayTemp(tempFile);

                IsPlaying = true;
                _timer.Start();
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert(
                        "Error",
                        $"Failed to play: {ex.Message}",
                        "OK"));
            }
        }

        [RelayCommand]
        private async Task DownloadSong(YoutubeVideo video)
        {
            if (video == null)
                return;

            try
            {
                var localPath = await _yt.DownloadAudioAsync(video.Url);

                await _downloadStorage.AddAsync(new DownloadSong
                {
                    Title = video.Title,
                    FilePath = localPath,
                    ThumbnailPath = video.ThumbnailUrl,
                    Uploader = video.Uploader
                });

                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert(
                        "Downloaded",
                        "Song saved for offline playback",
                        "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert(
                        "Error",
                        $"Download failed: {ex.Message}",
                        "OK"));
            }
        }

        public void PlayFile(string filePath, string title)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            CurrentFilePath = filePath;
            SongName = title;

            _audioService.PlayDownloaded(filePath);

            TotalDuration = _audioService.Duration;
            Position = 0;

            _timer.Start();
            IsPlaying = true;
        }
    }
}