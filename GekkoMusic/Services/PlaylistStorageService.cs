using GekkoMusic.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GekkoMusic.Services
{
    public class PlaylistStorageService
    {
        private readonly string _filePath;

        public ObservableCollection<Playlist> Playlists { get; private set; }
            = new();

        public PlaylistStorageService()
        {
            _filePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "playlists.json");

            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
                return;

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<ObservableCollection<Playlist>>(json);

            if (data != null)
                Playlists = data;
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(
                Playlists,
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_filePath, json);
        }

       

    }



}
