using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp1.Models
{
    public class VideoInfoOwn
    {
        public string Url { get; set; }
        public List<int> Resolution { get; set; } = new List<int>();
        public List<int> BitRate { get; set; } = new List<int>();
        public int ResolutionChosen { get; set; }
        public int BitRateChosen { get; set; }
    }
}
