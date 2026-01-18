using GekkoMusic.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GekkoMusic.Services
{
    public class DownloadStorageService
    {
        private readonly string _filePath;

        public ObservableCollection<DownloadSong> Downloads { get; } = new();

        public DownloadStorageService()
        {
            _filePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "downloads.json");

            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<List<DownloadSong>>(json);

                if (data != null)
                {
                    foreach (var song in data)
                    {
                        // Only add if file still exists
                        if (File.Exists(song.FilePath))
                            Downloads.Add(song);
                    }
                }
            }
            catch
            {
                // Ignore load errors
            }
        }

        public async Task AddAsync(DownloadSong song)
        {
            if (Downloads.Any(s => s.FilePath == song.FilePath))
                return;

            Downloads.Add(song);
            await SaveAsync();
        }

        public async Task RemoveAsync(DownloadSong song)
        {
            Downloads.Remove(song);
            await SaveAsync();

            // Delete the file
            try
            {
                if (File.Exists(song.FilePath))
                    File.Delete(song.FilePath);
            }
            catch
            {
                // Ignore deletion errors
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(
                    Downloads.ToList(),
                    new JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync(_filePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}