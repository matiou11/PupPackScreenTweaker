using System;
using System.Drawing;
using System.IO;
using Vlc.DotNet.Core;

namespace PupPackScreenTweaker
{
    public class Video
    {
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Duration { get; set; }

        /// <summary>
        /// Class describing a video file, manipulated using VLC libraries
        /// </summary>
        /// <param name="fileName"></param>
        public Video(string fileName)
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
                    if ((DateTime.UtcNow - start).TotalMilliseconds >= 3000) throw (new Exception("Error when starting video!"));
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
}
