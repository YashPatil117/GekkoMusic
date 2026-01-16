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
        // CONTROL METHODS
        // ======================
        //gpt generated
        public async Task LoadAsync(string source)
        {
            if (_player != null)
            {
                _player.Stop();
            }


            Stream stream;

            if (source.StartsWith("http"))
            {
                // ❌ DO NOT HTTP DOWNLOAD
                // source is now a local file path produced by yt-dlp
                stream = File.OpenRead(source);
            }
            else
            {
                stream = File.OpenRead(source);
            }

            _player = _audioManager.CreatePlayer(stream);
        }




        public async Task Play(string filePath)
        {
            _player?.Stop();
            _player?.Dispose();

            var stream = File.OpenRead(filePath);
            _player = AudioManager.Current.CreatePlayer(stream);
           // _playerVM.Thumbnail = song.ThumbnailPath;
            await Task.Delay(200);

            _player.Play();
        }
        public void Pause() => _player?.Pause();
        public void Stop()
        {
            if (_player == null)
                return;

            _player.Stop();
            _player.Dispose();
            _player = null;
        }

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
