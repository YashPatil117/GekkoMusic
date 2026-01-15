using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekkoMusic.ViewModels
{
    public class Playlist
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        // LOCAL file path to image
        public string? CoverImagePath { get; set; }
        
        public List<YoutubeVideo> Videos { get; set; } = new();
        //public ObservableCollection<YoutubeVideo> Songs { get; set; } = new();
    }

}
