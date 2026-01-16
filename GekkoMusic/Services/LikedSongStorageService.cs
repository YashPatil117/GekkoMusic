using GekkoMusic.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace GekkoMusic.Services
{
    public class LikedSongStorageService
    {
        private readonly string _filePath;

        public LikedSongStorageService()
        {
            _filePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "liked_songs.json");
        }

        public async Task<List<YoutubeVideo>> LoadAsync()
        {
            if (!File.Exists(_filePath))
                return new List<YoutubeVideo>();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<YoutubeVideo>>(json)
                   ?? new List<YoutubeVideo>();
        }

        public async Task SaveAsync(List<YoutubeVideo> songs)
        {
            var json = JsonSerializer.Serialize(songs, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_filePath, json);
        }

        // 🔴 THIS IS WHAT YOU WERE MISSING
        public async Task RemoveAsync(YoutubeVideo video)
        {
            var songs = await LoadAsync();

            var existing = songs.FirstOrDefault(x => x.Id == video.Id);
            if (existing != null)
            {
                songs.Remove(existing);
                await SaveAsync(songs);
            }
        }
        public async Task AddAsync(YoutubeVideo video)
        {
            var songs = await LoadAsync();

            // prevent duplicates
            if (songs.Any(x => x.Id == video.Id))
                return;

            songs.Add(video);
            await SaveAsync(songs);
        }

    }
}
