using BlazorApp1.Models;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VideoLibrary;
using Xabe.FFmpeg.Downloader;
using static BlazorApp1.Pages.MyPages.YtDownloader;

namespace BlazorApp1.Services
{
    public class YtDownloaderService : IYtDownloaderService
    {
        private List<YouTubeVideo> _videoVariants { get; set; }
        public List<int> Resolution
        {
            get
            {
                if (_videoVariants != null)
                    return _videoVariants
                    .Where(r => r.AdaptiveKind == AdaptiveKind.Video && r.Format == VideoFormat.Mp4)
                    .Select(r => r.Resolution).Distinct().ToList();
                return new List<int>();
            }
        }
        public List<int> BitRate
        {
            get
            {
                if (_videoVariants != null)
                    return _videoVariants
                    .Where(r => r.AdaptiveKind == AdaptiveKind.Audio)
                    .Select(r => r.AudioBitrate).Distinct().ToList();
                return new List<int>();
            }
        }

        public YtDownloaderService(string link = "", bool playlist = false)
        {
            try
            {
                _videoVariants = new YouTube().GetAllVideos(link).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _videoVariants = new YouTube().GetAllVideos("https://www.youtube.com/watch?v=SNBOb52uNH0&ab_channel=AzrealCodeLab").ToList();
            }
        }
        public object GetVideoData(string link, bool playlist = false)
        {
            YouTube yt = new YouTube();
            var videoData = yt.GetAllVideos(link).ToList();
            var resolution = videoData.Where(r => r.AdaptiveKind == AdaptiveKind.Video && r.Format == VideoFormat.Mp4).
                             Select(r => r.Resolution).ToList();
            var bitRate = videoData.Where(r => r.AdaptiveKind == AdaptiveKind.Audio).Select(r => r.AudioBitrate).ToList();
            return new { resolution, bitRate };
        }

        public async Task DownloadVideo(VideoInfoOwn videoInfo, DelegateChangeProgressBar ChangeProgressBar)
        {
            var totalbytes = 0;
            var collctedbytes = 0;
            string audiomp4 = "Audio.mp4";
            string Audiomp3 = "Audio.mp3";
            string VideoFile = "Video.mp4";
            var txtFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            async Task SourceDownloader(string name, YouTubeVideo vid)
            {
                var client = new HttpClient();
                long? totalByte = 0;
                using (Stream output = File.OpenWrite(name))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, vid.Uri))
                    {
                        totalByte = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength;
                    }
                    totalbytes += (int)totalByte;
                    using (var input = await client.GetStreamAsync(vid.Uri))
                    {
                        //byte[] buffer = new byte[16 * 1024];
                        byte[] buffer = new byte[256 * 1024 * 1024];
                        int read;
                        int totalRead = 0;

                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                            totalRead += read;
                            collctedbytes += read;
                            long x = collctedbytes * 100 / totalbytes;
                            ChangeProgressBar(collctedbytes, totalbytes);
                            Console.WriteLine(x);
                            //Dataprogress.Text = ByteConverter(collctedbytes) + "/" + ByteConverter(totalbytes);
                            //progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = (int)x));
                        }
                    }
                }
                client.Dispose();
            }
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);
            async Task DownloadWork(string link, int playlist = -1)
            {
                var video = YouTube.Default.GetAllVideos(link);
                var Targetaudio = video.Where(r => r.AdaptiveKind == AdaptiveKind.Audio &&
                r.AudioBitrate == videoInfo.BitRateChosen).Select(r => r);

                var TargetVideo = video.Where(r => r.AdaptiveKind == AdaptiveKind.Video &&
                r.Format == VideoFormat.Mp4 && r.Resolution == videoInfo.ResolutionChosen).Select(r => r);

                Task au = SourceDownloader(audiomp4, Targetaudio.ToList()[0]);

                Task vide = SourceDownloader(VideoFile, TargetVideo.ToList()[0]);
                await au;
                FFMpeg.ExtractAudio(audiomp4, Audiomp3);
                //File.Delete(audiomp4);
                await vide;
                FFMpeg.ReplaceAudio(VideoFile, Audiomp3, txtFilePath + TargetVideo.ToList()[0].FullName);
                //FileDelete(VideoFile);
                //FileDelete(Audiomp3);
            }
            await DownloadWork(videoInfo.Url);
        }

        private void FileDelete(string pa)
        {
            if (File.Exists(pa))
                File.Delete(pa);
        }
    }
}
