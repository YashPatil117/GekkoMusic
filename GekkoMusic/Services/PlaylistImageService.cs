using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekkoMusic.Services
{
    public static class PlaylistImageService
    {
        public static async Task<string?> PickAndSaveImageAsync(string playlistId)
        {
            var result = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = "Select playlist cover image",
                    FileTypes = FilePickerFileType.Images
                });

            if (result == null)
                return null;

            var destFolder = Path.Combine(
                FileSystem.AppDataDirectory,
                "Playlists",
                playlistId);

            Directory.CreateDirectory(destFolder);
            Debug.WriteLine(destFolder);

            var destPath = Path.Combine(destFolder, "cover" + Path.GetExtension(result.FileName));

            await using var src = await result.OpenReadAsync();
            await using var dst = File.OpenWrite(destPath);

            await src.CopyToAsync(dst);

            return destPath;
        }
    }

}
