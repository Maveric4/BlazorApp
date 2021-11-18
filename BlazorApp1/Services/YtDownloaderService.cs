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

        public async Task DownloadVideo(VideoInfoOwn videoInfo)
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
                        byte[] buffer = new byte[16 * 1024];
                        int read;
                        int totalRead = 0;

                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                            totalRead += read;
                            collctedbytes += read;
                            long x = collctedbytes * 100 / totalbytes;
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
                //if (playlist < 0)
                //    Status.Text = "Downloading...";
                //else
                //    Status.Text = (playlist + 1).ToString() + "/" + videoId.Count.ToString();
                var video = YouTube.Default.GetAllVideos(link);
                var Targetaudio = video.Where(r => r.AdaptiveKind == AdaptiveKind.Audio &&
                r.AudioBitrate == videoInfo.BitRateChosen).Select(r => r);

                var TargetVideo = video.Where(r => r.AdaptiveKind == AdaptiveKind.Video &&
                r.Format == VideoFormat.Mp4 && r.Resolution == videoInfo.ResolutionChosen).Select(r => r);

                //txtTitle.Invoke((MethodInvoker)(() => txtTitle.Text = video.ToList()[0].Title));
                Task au = SourceDownloader(audiomp4, Targetaudio.ToList()[0]);
                //if (chkAudioOnly.Checked != true)
                //{
                    Task vide = SourceDownloader(VideoFile, TargetVideo.ToList()[0]);
                    await au;
                    FFMpeg.ExtractAudio(audiomp4, Audiomp3);
                    //File.Delete(audiomp4);
                    await vide;
                    FFMpeg.ReplaceAudio(VideoFile, Audiomp3, txtFilePath + TargetVideo.ToList()[0].FullName);
                    //FileDelete(VideoFile);
                    //FileDelete(Audiomp3);
                //}
                //else
                //{
                //    await au;
                //    FFMpeg.ExtractAudio(audiomp4, txtFilePath.Text + TargetVideo.ToList()[0].Title + "mp3");
                //    FileDelete(audiomp4);
                //}
                //if (playlist < 0)
                //    Status.Text = "Completed";
                //else
                //    Status.Text = "Done (" + (playlist + 1).ToString() + "/" + videoId.Count.ToString() + ")";
                //Dataprogress.Text = "";
            }
            //if (videoId.Count > 0)
            //{
            //    for (int i = 0; i < videoId.Count; i++)
            //    {
            //        await DownloadWork(watchLink + videoId.ElementAt(i), i);
            //    }
            //}
            //else
            //{
                await DownloadWork(videoInfo.Url);
            //}
        }



        private void FileDelete(string pa)
        {
            if (File.Exists(pa))
                File.Delete(pa);
        }












        //public void Download(VideoInfoOwn video)
        //{
        //    try
        //    {
        //        string sURL = "large - webm";



        //        if (string.IsNullOrEmpty(sURL))
        //        {
        //            return;
        //        }



        //        NameValueCollection urlParams = HttpUtility.ParseQueryString(sURL);



        //        string videoTitle = urlParams["title"] + " " + "large - webm";
        //        string videoFormt = HttpUtility.HtmlDecode(urlParams["type"]);
        //        videoFormt = videoFormt.Split(';')[0].Split('/')[1];



        //        string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        //        string sFilePath = string.Format(Path.Combine(downloadPath, "Downloads\\{0}.{1}"), videoTitle, videoFormt);



        //        WebClient webClient = new WebClient();
        //        //webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
        //        webClient.DownloadFileAsync(new Uri(sURL), sFilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        //lblMessage.Text = ex.Message;
        //        //lblMessage.ForeColor = Color.Red;
        //        return;
        //    }
        //}

        //public void Download2(VideoInfoOwn video)
        //{

        //    try
        //    {
        //        Uri videoUri = new Uri(video.Url);
        //        string videoID = HttpUtility.ParseQueryString(videoUri.Query).Get("v");
        //        string videoInfoUrl = "http://www.youtube.com/get_video_info?video_id=" + videoID;



        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(videoInfoUrl);
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();



        //        Stream responseStream = response.GetResponseStream();
        //        StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));



        //        string videoInfo = HttpUtility.HtmlDecode(reader.ReadToEnd());



        //        NameValueCollection videoParams = HttpUtility.ParseQueryString(videoInfo);



        //        if (videoParams["reason"] != null)
        //        {
        //            //lblMessage.Text = videoParams["reason"];
        //            return;
        //        }



        //        string[] videoURLs = videoParams["url_encoded_fmt_stream_map"].Split(',');



        //        foreach (string vURL in videoURLs)
        //        {
        //            string sURL = HttpUtility.HtmlDecode(vURL);



        //            NameValueCollection urlParams = HttpUtility.ParseQueryString(sURL);
        //            string videoFormat = HttpUtility.HtmlDecode(urlParams["type"]);



        //            sURL = HttpUtility.HtmlDecode(urlParams["url"]);
        //            sURL += "&signature=" + HttpUtility.HtmlDecode(urlParams["sig"]);
        //            sURL += "&type=" + videoFormat;
        //            sURL += "&title=" + HttpUtility.HtmlDecode(videoParams["title"]);



        //            videoFormat = urlParams["quality"] + " - " + videoFormat.Split(';')[0].Split('/')[1];



        //            //ddlVideoFormats.Items.Add(new ListItem(videoFormat, sURL));
        //        }



        //        //btnProcess.Enabled = false;
        //        //ddlVideoFormats.Visible = true;
        //        //btnDownload.Visible = true;
        //        //lblMessage.Text = string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        //lblMessage.Text = ex.Message;
        //        //lblMessage.ForeColor = Color.Red;
        //        return;
        //    }
        ////}   
    }
}
