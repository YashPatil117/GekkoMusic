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

namespace GekkoMusic.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly AudioPlayerService _audioService;
        private readonly IDispatcherTimer _timer;


        //Youtube property
        private readonly YoutubeDlpService _yt;

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
        YoutubeDlpService yt)
        {
            _audioService = audioService;
            _yt = yt;

            _timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(400);
            _timer.Tick += (_, _) => UpdatePosition();

            Volume = 0.7;
        }

        public async Task InitializeAsync()
        {
            await _audioService.LoadAsync("sample.mp3");
            TotalDuration = _audioService.Duration;
            SongName = "Sample Song";
        }

        [ObservableProperty] private double position;
        [ObservableProperty] private double totalDuration;
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

            var results = await _yt.SearchAsync(query, 3);

            foreach (var r in results)
                SearchResults.Add(r);
        }


    }

}
