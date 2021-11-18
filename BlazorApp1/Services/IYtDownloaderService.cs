using System.Collections.Generic;

namespace BlazorApp1.Services
{
    public interface IYtDownloaderService
    {
        List<int> Resolution { get; }

        object GetVideoData(string link, bool playlist = false);
    }
}