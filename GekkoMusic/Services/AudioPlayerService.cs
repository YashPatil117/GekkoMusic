using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GekkoMusic.Services;
using System.Timers;

namespace GekkoMusic.Services
{
    

    

    public class AudioPlayerService
    {
        private IAudioPlayer? _player;
        private readonly IAudioManager _audioManager;
        private static readonly HttpClient _httpClient = new();

        public AudioPlayerService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        // ======================
        // READ-ONLY STATE
        // ======================

        public bool IsPlaying => _player?.IsPlaying == true;
        public double Duration => _player?.Duration ?? 0;
        public double Position => _player?.CurrentPosition ?? 0;
        public double Volume
        {
            get => _player?.Volume ?? 0;
            set
            {
                if (_player != null)
                    _player.Volume = value;
            }
        }
        // ======================
        // CONTROL METHODS
        // ======================

        public async Task LoadAsync(string source)
        {
            if (_player != null)
                return;

            Stream stream = source.StartsWith("http")
                ? await _httpClient.GetStreamAsync(source)
                : await FileSystem.OpenAppPackageFileAsync(source);

            _player = _audioManager.CreatePlayer(stream);
        }

        public void Play() => _player?.Play();
        public void Pause() => _player?.Pause();

        public void Seek(double position)
        {
            _player?.Seek(position);
        }

        public void SetVolume(double volume)
        {
            if (_player != null)
                _player.Volume = volume;
        }
    }

}
