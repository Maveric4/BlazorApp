using System.Collections.Generic;

namespace BlazorApp1.Models
{
    public class VideoInfoOwn
    {
        public string Url { get; set; }
        public List<int> Resolution { get; set; } = new List<int>();
        public List<int> BitRate { get; set; } = new List<int>();
        public int ResolutionChosen { get; set; }
        public int BitRateChosen { get; set; }
        public float Progress { get; set; }
    }
}
