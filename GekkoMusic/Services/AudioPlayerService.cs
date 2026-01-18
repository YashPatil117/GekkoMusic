using GekkoMusic.ViewModels;
using Plugin.Maui.Audio;
using System;
using System.IO;

namespace GekkoMusic.Services
{
    public class AudioPlayerService
    {
        private IAudioPlayer? _player;
        private Stream? _stream;
        private readonly IAudioManager _audioManager;
        private string? _currentTempFile;
        private bool _isStopping;

        public AudioPlayerService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        // ======================
        // STATE
        // ======================
        public bool IsPlaying => _player?.IsPlaying == true;
        public double Duration => _player?.Duration ?? 0;
        public double Position => _player?.CurrentPosition ?? 0;
        public double Volume
        {
            get => _player?.Volume ?? 1;
            set
            {
                if (_player != null)
                    _player.Volume = value;
            }
        }

        // ======================
        // PLAY METHODS
        // ======================

        /// <summary>
        /// Play permanently downloaded music
        /// </summary>
        public void PlayDownloaded(string filePath)
        {
            PlayInternal(filePath, isTemp: false);
        }

        /// <summary>
        /// Play temporary (online) music
        /// </summary>
        public void PlayTemp(string filePath)
        {
            PlayInternal(filePath, isTemp: true);
        }

        private void PlayInternal(string filePath, bool isTemp)
        {
            // CRITICAL: Stop previous playback FIRST
            StopInternal();

            if (!File.Exists(filePath))
                return;

            _currentTempFile = isTemp ? filePath : null;

            // Open with FileShare.Read to prevent locking
            _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            _player = _audioManager.CreatePlayer(_stream);
            _player.PlaybackEnded += OnPlaybackEnded;
            _player.Play();
        }

        // ======================
        // CONTROLS
        // ======================

        public void Pause() => _player?.Pause();

        public void Resume() => _player?.Play();

        public void TogglePlay()
        {
            if (_player == null)
                return;

            if (_player.IsPlaying)
                _player.Pause();
            else
                _player.Play();
        }

        public void Seek(double position)
        {
            if (_player != null && position >= 0 && position <= Duration)
                _player.Seek(position);
        }

        // ======================
        // STOP + CLEANUP
        // ======================

        public void Stop()
        {
            StopInternal();
        }

        private void StopInternal()
        {
            if (_isStopping)
                return;

            _isStopping = true;

            try
            {
                if (_player != null)
                {
                    _player.PlaybackEnded -= OnPlaybackEnded;

                    try
                    {
                        if (_player.IsPlaying)
                            _player.Stop();
                    }
                    catch { }

                    _player.Dispose();
                    _player = null;
                }

                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Delete temp file async
                if (!string.IsNullOrEmpty(_currentTempFile))
                {
                    var tempFile = _currentTempFile;
                    _currentTempFile = null;

                    Task.Run(async () =>
                    {
                        await Task.Delay(500);
                        for (int i = 0; i < 10; i++)
                        {
                            try
                            {
                                if (File.Exists(tempFile))
                                    File.Delete(tempFile);
                                break;
                            }
                            catch
                            {
                                if (i < 9)
                                    await Task.Delay(500);
                            }
                        }
                    });
                }
            }
            finally
            {
                _isStopping = false;
            }
        }

        private void OnPlaybackEnded(object? sender, EventArgs e)
        {
            // Don't delete temp file when song ends naturally
            // Only dispose player and stream
            if (_player != null)
            {
                _player.PlaybackEnded -= OnPlaybackEnded;
                _player.Dispose();
                _player = null;
            }

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            // Keep _currentTempFile - will be deleted only when playing new song
        }

        private void CleanupTempFile()
        {
            if (string.IsNullOrEmpty(_currentTempFile))
                return;

            if (!File.Exists(_currentTempFile))
            {
                _currentTempFile = null;
                return;
            }

            // Retry deletion with delays
            int maxRetries = 5;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Delete(_currentTempFile);
                    _currentTempFile = null;
                    return;
                }
                catch
                {
                    if (i < maxRetries - 1)
                        System.Threading.Thread.Sleep(200);
                }
            }

            // If all retries fail, schedule for later cleanup
            _currentTempFile = null;
        }

        // ======================
        // CLEANUP ALL TEMP FILES ON APP START
        // ======================

        public static void CleanupAllTempFiles()
        {
            var tempDir = MusicDirectories.TempMusic;

            if (!Directory.Exists(tempDir))
                return;

            try
            {
                foreach (var file in Directory.GetFiles(tempDir, "*.mp3"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore individual file errors
                    }
                }
            }
            catch
            {
                // Ignore directory access errors
            }
        }
    }
}