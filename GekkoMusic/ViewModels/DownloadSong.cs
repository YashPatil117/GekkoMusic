using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekkoMusic.ViewModels
{
    public class DownloadSong
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; } // optional

        public string Uploader { get; set; }
    }
   
    public static class MusicDirectories
    {
        public static string Downloads =>
            Path.Combine(FileSystem.AppDataDirectory, "Downloads");
    }

}
