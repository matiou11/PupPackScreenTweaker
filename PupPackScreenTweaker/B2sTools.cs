using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace CustomPos
{
    /// <summary>
    /// Class describing a B2S file (backglass for Visual Pinball)
    /// </summary>
    public class B2s
    {
        public int GrillHeight { get; set; }
        public string FileName { get; set; }
        public bool IsValid { get; set; }
        public bool HasGrill()
        {
            return (GrillHeight > 0);
        }
        private bool hasOnBg;
        private bool hasOffBg;
        private bool hasBg;

        public Image BackGlassImage(bool withGrill)
        {
            Image pic = B2sTools.StringToImage(getImageXML(getImageTypeToUse()));
            if (!withGrill && pic != null)
            {
                pic = B2sTools.CropImage(pic, new Rectangle(0, 0, pic.Width, pic.Height - GrillHeight));
            }
            return pic;
        }

        public Image GrillImage()
        {
            Image pic = BackGlassImage(true);
            if (pic != null) pic = B2sTools.CropImage(pic, new Rectangle(0, pic.Height - GrillHeight, pic.Width, GrillHeight));
            return pic;
        }

        private string getImageTypeToUse()
        {
            if (hasBg) return "BackglassImage";
            if (hasOnBg) return "BackglassOnImage";
            if (hasOffBg) return "BackglassOffImage";
            return "";
        }

        public B2s(string fileName)
        {
            this.FileName = fileName;
            this.hasBg = false;
            this.hasOffBg = false;
            this.hasOnBg = false;
            this.GrillHeight = 0;
            this.IsValid = false;
            this.Init();
        }

        private string getImageXML(string imageType)
        {
            string img;

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(FileName);
                XmlNode topNode = xml.SelectSingleNode("DirectB2SData");
                XmlNode imagesNode = topNode.SelectSingleNode("Images");
                XmlNode ImageXml = imagesNode.SelectSingleNode(imageType);
                img = ImageXml.Attributes["Value"].InnerText;
            }
            catch
            {
                img = "";
            }
            return img;
        }

        private void Init()
        {
            try
            {
                XmlDocument XML = new XmlDocument();
                XML.Load(FileName);
                XmlNode topNode = XML.SelectSingleNode("DirectB2SData");
                try
                {
                    GrillHeight = Convert.ToInt32(topNode.SelectSingleNode("GrillHeight").Attributes["Value"].InnerText);
                }
                catch
                {
                    GrillHeight = 0;
                }

                XmlNode imagesNode = topNode.SelectSingleNode("Images");

                hasBg = imagesNode.SelectSingleNode("BackglassImage") != null;
                hasOffBg = imagesNode.SelectSingleNode("BackglassOffImage") != null;
                hasOnBg = imagesNode.SelectSingleNode("BackglassOnImage") != null;

                IsValid = true;

            }
            catch
            {
                IsValid = false;
            }


        }


    }

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
