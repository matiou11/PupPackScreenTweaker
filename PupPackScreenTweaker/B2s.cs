using System;
using System.Drawing;
using System.Xml;

namespace PupPackScreenTweaker
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
}
