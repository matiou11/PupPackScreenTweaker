using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace PupPackScreenTweaker
{
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
