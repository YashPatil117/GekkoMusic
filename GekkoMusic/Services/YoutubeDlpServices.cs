using GekkoMusic.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace GekkoMusic.Services
{
    public class YoutubeDlpService
    {
        private readonly string _ytDlpPath;

        public YoutubeDlpService()
        {
            _ytDlpPath = Path.Combine(
                AppContext.BaseDirectory,
                "Tools",
                "yt-dlp.exe"
            );
        }

        public async IAsyncEnumerable<YoutubeVideo> SearchStreamAsync(
           string query, int limit = 5)
        {
            var args =
                $"ytsearch{limit}:\"{query}\" " +
                "--dump-json --skip-download --quiet";

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)!;

            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var video = JsonSerializer.Deserialize<YoutubeVideo>(line);
                if (video != null)
                    yield return video;
            }

            await process.WaitForExitAsync();
        }

        public async Task<string> DownloadAudioAsync(string youtubeUrl)
        {
            var downloadsDir = MusicDirectories.Downloads;
            Directory.CreateDirectory(downloadsDir);

            var outputTemplate = Path.Combine(downloadsDir, $"{Guid.NewGuid()}.%(ext)s");

            if (!File.Exists(_ytDlpPath))
                throw new FileNotFoundException("yt-dlp.exe not found", _ytDlpPath);

            var args =
                $"-f bestaudio " +
                $"--no-playlist " +
                $"-o \"{outputTemplate}\" " +
                $"\"{youtubeUrl}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp failed: {stderr}");

            // Find the downloaded file (could be .webm, .m4a, .opus, etc.)
            var directory = Path.GetDirectoryName(outputTemplate);
            var filePattern = Path.GetFileNameWithoutExtension(outputTemplate) + ".*";
            var files = Directory.GetFiles(directory, filePattern);

            if (files.Length == 0)
                throw new FileNotFoundException("Download failed - no file created");

            var outputPath = files[0];

            // Wait for file to be fully written
            var timeout = DateTime.UtcNow.AddSeconds(10);
            while (new FileInfo(outputPath).Length == 0 && DateTime.UtcNow < timeout)
            {
                await Task.Delay(200);
            }

            if (new FileInfo(outputPath).Length == 0)
                throw new FileNotFoundException("Download failed - file is empty", outputPath);

            process.Dispose();

            return outputPath;
        }

        public async Task<string> DownloadTempAsync(string url)
        {
            Directory.CreateDirectory(PathPlayer.TempMusic);

            var outputTemplate = Path.Combine(
                PathPlayer.TempMusic,
                $"{Guid.NewGuid()}.%(ext)s"  // Changed from .mp3 to .%(ext)s
            );

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                Arguments =
                    $"-f bestaudio " +  // Removed --extract-audio and --audio-format
                    $"--no-playlist " +
                    $"-o \"{outputTemplate}\" " +
                    $"\"{url}\"",

                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new Exception("Failed to start yt-dlp");

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("yt-dlp exited with error");

            // Find the downloaded file
            var directory = Path.GetDirectoryName(outputTemplate);
            var filePattern = Path.GetFileNameWithoutExtension(outputTemplate) + ".*";
            var files = Directory.GetFiles(directory, filePattern);

            if (files.Length == 0)
                throw new Exception("yt-dlp did not create any file");

            var outputPath = files[0];

            // Wait for file to be ready
            var timeout = DateTime.UtcNow.AddSeconds(10);
            while (new FileInfo(outputPath).Length == 0 && DateTime.UtcNow < timeout)
            {
                await Task.Delay(200);
            }

            if (new FileInfo(outputPath).Length == 0)
                throw new Exception("yt-dlp output file invalid");

            return outputPath;
        }
    }
}