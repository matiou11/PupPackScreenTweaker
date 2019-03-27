using System;
using System.Drawing;
using System.IO;

namespace PupPackScreenTweaker
{

    /// <summary>
    /// Various tools to manipulate B2S data
    /// </summary>
    static class B2sTools
    {
        public static Image StringToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public static Image CropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        /// <summary>
        /// Create a .res file, used to define custom positioning of a B2S
        /// </summary>
        /// <param name="pupScreen"></param>
        /// <param name="refScreens"></param>
        /// <returns></returns>
        public static string BuildResFile(PupScreen pupScreen, PupScreens refScreens)
        {
            string content = "";
            content += refScreens[3].W.ToString() + Environment.NewLine;
            content += refScreens[3].H.ToString() + Environment.NewLine;
            content += pupScreen.W.ToString() + Environment.NewLine;
            content += pupScreen.H.ToString() + Environment.NewLine;
            content += "@" + refScreens[2].X.ToString() + Environment.NewLine;
            content += (pupScreen.X - refScreens[2].X).ToString() + Environment.NewLine;
            //content += (pupScreen.Y - refScreens[2].Y).ToString() + Environment.NewLine;
            content += (pupScreen.Y - 0).ToString() + Environment.NewLine;
            content += refScreens[1].W.ToString() + Environment.NewLine;
            content += refScreens[1].H.ToString() + Environment.NewLine;
            content += (refScreens[1].X - refScreens[2].X).ToString() + Environment.NewLine;
            content += (refScreens[1].Y - 0).ToString() + Environment.NewLine;
            content += "0" + Environment.NewLine;
            content += (pupScreen.X - refScreens[2].X).ToString() + Environment.NewLine;
            content += (pupScreen.Y - 0).ToString() + Environment.NewLine;
            content += pupScreen.W.ToString();
            content += pupScreen.H.ToString();
            content += "" + Environment.NewLine;
            return content;
        }
    }
}
