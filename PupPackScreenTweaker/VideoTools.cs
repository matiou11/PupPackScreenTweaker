using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Vlc.DotNet.Core;

namespace CustomPos
{
    public class Vid
    {
        public string FileName { get; set; } 
        public int Width { get; set; }
        public int Height { get; set; }
        public double Duration { get; set; }

        /// <summary>
        /// Class describing a video file, manipulated using VLC libraries
        /// </summary>
        /// <param name="fileName"></param>
        public Vid(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            bool initialized = false;
            if (!VideoTools.checkVlcLib()) throw new Exception("Cannot find the VLC library... Please make sure you're running this application from inside your PinUp folder...");
            try
            {
                VlcMediaPlayer mediaPlayer = new VlcMediaPlayer(VideoTools.GetLibVlcLocation(), VideoTools.GetVlcOptionsHeadless(0));
                initialized = true;
                mediaPlayer.SetMedia(file);
                mediaPlayer.GetMedia().Parse();
                this.Duration = mediaPlayer.GetMedia().Duration.TotalSeconds;
                this.Width = (int)mediaPlayer.GetMedia().TracksInformations[0].Video.Width;
                this.Height = (int)mediaPlayer.GetMedia().TracksInformations[0].Video.Height;
                this.FileName = fileName;
                mediaPlayer.Dispose();
            }
            catch (Exception exc)
            {
                if (!initialized)
                {
                    throw (new Exception("Cannot initialize VLC player library: " + exc.Message));
                }
                else
                {
                    throw (new Exception("VLC library error: " + exc.Message));
                }
            }
        }

        /// <summary>
        /// Extract a still frame from a video
        /// </summary>
        /// <param name="startAt"></param>
        /// <returns></returns>
        public Bitmap GetFrame(double startAt)
        {
            FileInfo file = new FileInfo(this.FileName);
            FileInfo file2 = new FileInfo(Path.GetTempPath() + "\\snapshot.png");
            if (File.Exists(file2.FullName))
            {
                File.Delete(file2.FullName);
            }
            VlcMediaPlayer mediaPlayer = new VlcMediaPlayer(VideoTools.GetLibVlcLocation(), VideoTools.GetVlcOptionsHeadless(startAt));
            try
            {
                bool done = false;
                mediaPlayer.PositionChanged += (sender, e) =>
                {
                    mediaPlayer.TakeSnapshot(file2);
                    done = true;
                };
                mediaPlayer.SetMedia(file);
                mediaPlayer.Play();
                DateTime start = DateTime.UtcNow;
                while (!done) // wait for video to start, timeout 3 seconds
                {
                    if ((DateTime.UtcNow - start).TotalMilliseconds >= 3000) throw(new Exception("Error when starting video!"));
                }
            }
            catch (Exception exc)
            {
                throw (new Exception("VLC library error:" + exc.Message));
            }
            if (File.Exists(file2.FullName))
            {
                Bitmap pic = new Bitmap(file2.FullName);
                return pic;
            }
            else
            {
                throw (new Exception("Error while extracting snapshot from video!"));
            }
        }
    }

    public static class VideoTools
    {
        /// <summary>
        /// Where to find the VLC installation
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetLibVlcLocation()
        {
            var libDirectory = new DirectoryInfo(Path.Combine(".\\VLC"));
            return libDirectory;
        }

        public static bool checkVlcLib()
        {
            return File.Exists(GetLibVlcLocation() + "\\vlc.exe");
        }

        /// <summary>
        /// VLC options needed to play a video in headless mode, with no sound
        /// </summary>
        /// <param name="startAt"></param>
        /// <returns></returns>
        public static string[] GetVlcOptionsHeadless(double startAt)
        {
            string[] options = new[]
            {
                "--intf", "dummy", /* no interface                   */
                "--vout", "dummy", /* we don't want video output     */
                "--no-audio", /* we don't want audio decoding   */
                "--no-video-title-show", /* nor the filename displayed     */
                "--no-stats", /* no stats */
                "--no-sub-autodetect-file", /* we don't want subtitles        */
                "--no-snapshot-preview", /* no blending in dummy vout      */
                "--start-time=" + startAt.ToString()
            };
            return options;
        }

        public static Bitmap CopyAndReleaseImage(Bitmap source)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] byteArray;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                source.Save(memoryStream, ImageFormat.Png);
                byteArray = memoryStream.ToArray();
            }
            Bitmap targetPic = (Bitmap)_imageConverter.ConvertFrom(byteArray);
            if (targetPic != null && (targetPic.HorizontalResolution != (int)targetPic.HorizontalResolution ||
                               targetPic.VerticalResolution != (int)targetPic.VerticalResolution))
            {
                targetPic.SetResolution((int)(targetPic.HorizontalResolution + 0.5f),
                                 (int)(targetPic.VerticalResolution + 0.5f));
            }
            source.Dispose();
            return targetPic;
        }
    }
}
