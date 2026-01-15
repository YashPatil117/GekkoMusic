using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GekkoMusic.Services;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GekkoMusic.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly AudioPlayerService _audioService;

        private readonly IDispatcherTimer _timer;

        private readonly PlaylistStorageService _storage;


        //Youtube property
        private readonly YoutubeDlpService _yt;

        public ObservableCollection<Playlist> Playlists => _storage.Playlists;


        public bool HasPlaylists => Playlists.Any();

        //public PlayerViewModel(AudioPlayerService audioService)
        //{
        //    _audioService = audioService;

        //    _timer = Application.Current!.Dispatcher.CreateTimer();
        //    _timer.Interval = TimeSpan.FromMilliseconds(400);
        //    _timer.Tick += (_, _) => UpdatePosition();

        //    Volume = 0.7;
        //}


        public PlayerViewModel(
        AudioPlayerService audioService,
        YoutubeDlpService yt,
        PlaylistStorageService storage)
        {
            _audioService = audioService;
            _yt = yt;

            _timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(400);
            _timer.Tick += (_, _) => UpdatePosition();

            Volume = 0.7;
            _storage = storage;
            Playlists.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPlaylists));
        }

        public async Task InitializeAsync()
        {
            await _audioService.LoadAsync("sample.mp3");
            TotalDuration = _audioService.Duration;
            SongName = "Sample Song";
        }

        [ObservableProperty] 
        private double position;

        [ObservableProperty] 
        private double totalDuration;

        [ObservableProperty]
        private string searchText = string.Empty;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PlayPauseIcon))]
        private bool isPlaying;
            
        [ObservableProperty] private bool isDragging;
        [ObservableProperty] private double volume;
        [ObservableProperty] private string songName = string.Empty;

        public string CurrentTime => FormatTime(Position);
        public string TotalTime => FormatTime(TotalDuration);
        public string PlayPauseIcon => IsPlaying ? "pause_player.png" : "play_player.png";

        [RelayCommand]
        private void PlayPause()
        {
            if (_audioService.IsPlaying)
            {
                _audioService.Pause();
                _timer.Stop();
                IsPlaying = false;
            }
            else
            {
                _audioService.Play();
                _timer.Start();
                IsPlaying = true;
            }
        }

        [RelayCommand] private void SeekStarted() => IsDragging = true;

        [RelayCommand]
        private void SeekCompleted()
        {
            _audioService.Seek(Position);
            IsDragging = false;
        }

        private void UpdatePosition()
        {
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


        //YOUTUBES videos
        

        public ObservableCollection<YoutubeVideo> SearchResults { get; } = new();

        //public PlayerViewModel(YoutubeDlpService yt)
        //{
        //    _yt = yt;
        //}

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



        private void OpenPlaylist(Playlist playlist)
        {
            // Navigate to PlaylistDetailsPage
        }
        




    }

}
