using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GekkoMusic.ViewModels;
using System.Text.Json.Serialization;
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

        public async Task<List<YoutubeVideo>> SearchAsync(string query, int limit = 5)
        {
            var args =
            $"ytsearch{limit}:\"{query}\" " +
            "--dump-json " +
            "--skip-download " +
            $"--playlist-end {limit} " +
            "--quiet";

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi)!;

            var videos = new List<YoutubeVideo>();

            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                Debug.WriteLine("RAW yt-dlp JSON:");
                Debug.WriteLine(line);

                var video = JsonSerializer.Deserialize<YoutubeVideo>(line);
                if (video != null)
                    videos.Add(video);
            }


            await process.WaitForExitAsync();
            Debug.WriteLine($"TOTAL VIDEOS: {videos.Count}");

            foreach (var v in videos)
            {
                Debug.WriteLine("---------------");
                Debug.WriteLine($"Title   : {v.Title}");
                Debug.WriteLine($"ID      : {v.Id}");
                Debug.WriteLine($"Channel : {v.Uploader}");
                Debug.WriteLine($"Duration: {v.Duration}");
            }

            return videos;
        }
    }
}
