using GekkoMusic.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace GekkoMusic.Services
{
    public class YoutubeDlpService
    {
        private readonly string _ytDlpPath;
       // public ObservableCollection<YoutubeVideo> Videos { get; } = new ObservableCollection<YoutubeVideo>();

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

        public async Task<string> GetAudioStreamUrlAsync(string videoUrl)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,

                Arguments = $"-f bestaudio -g {videoUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)!;
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim(); //THIS IS A DIRECT AUDIO STREAM URL
        }



        //gpt generated
        public async Task<string> DownloadAudioAsync(string youtubeUrl)
        {
            var downloadsDir =
                Path.Combine(FileSystem.AppDataDirectory, "Downloads");

            Directory.CreateDirectory(downloadsDir);

            var outputPath =
                Path.Combine(downloadsDir, $"{Guid.NewGuid()}.mp3");

            var ytDlpPath =
                Path.Combine(AppContext.BaseDirectory, "Tools", "yt-dlp.exe");

            if (!File.Exists(ytDlpPath))
                throw new FileNotFoundException("yt-dlp.exe not found", ytDlpPath);

            var args =
                $"--js-runtimes node " +
                $"-f bestaudio\r\n " +
                $"--no-playlist " +
                $"-o \"{outputPath}\" " +
                $"\"{youtubeUrl}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
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

            if (!File.Exists(outputPath))
                throw new FileNotFoundException("Download failed", outputPath);

            return outputPath;
        }




    }
}

