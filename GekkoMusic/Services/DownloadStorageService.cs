using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GekkoMusic.ViewModels;

namespace GekkoMusic.Services
{
    public class DownloadStorageService
    {
        private readonly string _filePath;

        public ObservableCollection<DownloadSong> Downloads { get; }
            = new();

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

            var json = File.ReadAllText(_filePath);
            var data =
                JsonSerializer.Deserialize<List<DownloadSong>>(json);

            if (data != null)
                foreach (var song in data)
                    Downloads.Add(song);
        }

        public async Task AddAsync(DownloadSong song)
        {
            if (Downloads.Any(s => s.FilePath == song.FilePath))
                return;

            Downloads.Add(song);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(
                Downloads.ToList(),
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_filePath, json);
        }
    }

}
