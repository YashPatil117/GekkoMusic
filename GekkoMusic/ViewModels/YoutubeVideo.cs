using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace GekkoMusic.ViewModels
{
    

    public class YoutubeVideo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("uploader")]
        public string Uploader { get; set; } = "";

        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        public string ThumbnailUrl =>$"https://img.youtube.com/vi/{Id}/mqdefault.jpg";

        public string Url => $"https://www.youtube.com/watch?v={Id}";
    }

}
