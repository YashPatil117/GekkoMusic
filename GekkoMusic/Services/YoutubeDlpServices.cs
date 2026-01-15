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
    }
}

