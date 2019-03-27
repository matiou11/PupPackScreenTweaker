using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CustomPos
{
    public class PupScreen //: INotifyPropertyChanged
    {
        public int ScreenIndex { get; set; }
        public string Description { get; set; }
        public string PlayList { get; set; }
        public string Playfile { get; set; }
        public bool Loopit { get; set; }
        public string Priority { get; set; }
        public string Active { get; set; }

        // the collection of reference screens (never modified)
        List<PupScreen> refScreens;
        public static string[] refScreenNames = { "Topper", "DMD", "BackGlass", "Playfield", "Music", "Menu", "Game Select", "Other1", "Other2", "GameInfo", "GameHelp" };
        // the index of the reference screen used by this instance
        private int refScreenIndex;

        public double CustPosX { get; set; }
        public double CustPosY { get; set; }
        public double CustPosW { get; set; }
        public double CustPosH { get; set; }
        public bool HasCustomPos { get; set; }

        private string originalCsv;

        public MatDWindow Window { get; set; }

        public int X
        {
            get { return Window.Left; }
            set { Window.Left = value; }
        }
        public int Y
        {
            get { return Window.Top; }
            set { Window.Top = value; }
        }
        public int W
        {
            get { return Window.Width; }
            set { Window.Width = value; }
        }
        public int H
        {
            get { return Window.Height; }
            set { Window.Height = value; }
        }

        /// <summary>
        /// triggered if the pup screen moves or changes size
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public PupScreen(bool transparent, Color? borderColor, List<PupScreen> refScreens)
        {
            Window = new MatDWindow(false, transparent, true, null,borderColor);
            Window.PropertiesChanged += c_WindowChanged;
            this.refScreens = refScreens;
            HasCustomPos = false;
        }

        private void c_WindowChanged(object sender, EventArgs e)
        {
            // was it a "non custom pos" window that just got moved ? 
            // if yes, make it a "custom pos", with reference to same index as his
            if (!HasCustomPos)
            {
                // did it really move ?
                if (X != refScreens[ScreenIndex].X ||
                    Y != refScreens[ScreenIndex].Y ||
                    W != refScreens[ScreenIndex].W ||
                    H != refScreens[ScreenIndex].H)
                {
                    HasCustomPos = true;
                    //SetRefScreen(ScreenIndex);
                }
            }
            CalculateCustomPos();
            OnPropertyChanged(null);
        }

        private PupScreen refScreen()
        {
            foreach (PupScreen pupScreen in refScreens) if (pupScreen.ScreenIndex == refScreenIndex) return pupScreen;
            return null;
        }

        public void SetRefScreen(int index)
        {
            refScreenIndex = index;
            Window.SetLimits(refScreen().X, refScreen().X + refScreen().W, refScreen().Y, refScreen().Y + refScreen().H);
        }

        public int GetRefScreenIndex()
        {
            return refScreenIndex;
        }

        /// <summary>
        /// was the screen created during this session or loaded from csv ?
        /// </summary>
        /// <returns></returns>
        public bool IsNewlyCreated()
        {
            return (originalCsv == null || originalCsv == "") ;
        }

        /// <summary>
        /// get or set the CustomPos string
        /// </summary>
        public string CustomPos
        {
            get
            {
                if (HasCustomPos)
                {
                    return refScreenIndex.ToString() + "," +
                        Math.Round((double)CustPosX, 2).ToString(CultureInfo.InvariantCulture) + "," +
                        Math.Round((double)CustPosY, 2).ToString(CultureInfo.InvariantCulture) + "," +
                        Math.Round((double)CustPosW, 2).ToString(CultureInfo.InvariantCulture) + "," +
                        Math.Round((double)CustPosH, 2).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (value != "")
                {
                    string[] items = value.Trim('\"').Split(',');
                    refScreenIndex = Convert.ToInt16(items[0]);
                    CustPosX = Convert.ToSingle(items[1], CultureInfo.InvariantCulture);
                    CustPosY = Convert.ToSingle(items[2], CultureInfo.InvariantCulture);
                    CustPosW = Convert.ToSingle(items[3], CultureInfo.InvariantCulture);
                    CustPosH = Convert.ToSingle(items[4], CultureInfo.InvariantCulture);
                    HasCustomPos = true;
                }
                else
                {
                    CustPosX = 0;
                    CustPosY = 0;
                    CustPosW = 100;
                    CustPosH = 100;
                    refScreenIndex = ScreenIndex;
                    HasCustomPos = false;
                }
            }


        }

        /// <summary>
        /// Load the pup screen instance with values from a csv line from screens.pup
        /// </summary>
        /// <param name="csv"></param>
        public void LoadFromCsv(string csv)
        {
            originalCsv = csv;

            string[] items = PupTools.SplitCsv(csv);
            ScreenIndex = Convert.ToInt16(items[0]);
            Description = items[1];
            PlayList = items[2];
            Playfile = items[3];
            Loopit = (items[4]=="1");
            Active = items[5];
            Priority = items[6];
            if (items.Length > 7) CustomPos = items[7]; else CustomPos = "";
            Window.SetCaption(PupScreens.DEFAULT_SCREEN_CAPTION + ScreenIndex);
            SetRefScreen(refScreenIndex);
        }

        /// <summary>
        /// Build a csv line describing this pup screen (to be saved in a screens.pup file)
        /// </summary>
        /// <returns></returns>
        public string GetCsv()
        {
            string text = "";
            text += ScreenIndex.ToString() + ",";
            text += "\"" + Description + "\",";
            text += "\"" + PlayList + "\",";
            text += "\"" + Playfile + "\",";
            text += (Loopit?"1":"") + ",";
            text += Active + ",";
            text += Priority + ",";
            text += "\"" + CustomPos + "\"";
            return text;
        }

        /// <summary>
        /// restore the settings of the pup screen when it was first loaded
        /// </summary>
        public void RestoreDefault()
        {
            LoadFromCsv(originalCsv);
            CalculateRealPos();
        }

        /// <summary>
        /// Calculate the custom pos fields according to the current coordinates/position
        /// </summary>
        public void CalculateCustomPos()
        {
            if (HasCustomPos)
            {
                CustPosX = Math.Round(X == 0 ? 0 : 100.0 / (double)refScreen().W * ((double)X - refScreen().X), 2);
                CustPosY = Math.Round(Y == 0 ? 0 : 100.0 / (double)refScreen().H * ((double)Y - refScreen().Y), 2);
                CustPosW = Math.Round(100.0 / (double)refScreen().W * (double)W, 2);
                CustPosH = Math.Round(100.0 / (double)refScreen().H * (double)H, 2);
            }
        }

        /// <summary>
        /// calculate the physical position.size of the window according to its CustomPos values
        /// </summary>
        public void CalculateRealPos()
        {
            if (HasCustomPos)
            {
                X = Convert.ToInt16((double)refScreen().W / 100.0 * (double)CustPosX + (double)refScreen().X);
                Y = Convert.ToInt16((double)refScreen().H / 100.0 * (double)CustPosY + (double)refScreen().Y);
                W = Convert.ToInt16((double)refScreen().W / 100.0 * (double)CustPosW);
                H = Convert.ToInt16((double)refScreen().H / 100.0 * (double)CustPosH);
            }
            else
            {
                X = refScreen().X;
                Y = refScreen().Y;
                W = refScreen().W;
                H = refScreen().H;
            }
        }
        /// <summary>
        /// get a description of this pup screen as a list item
        /// </summary>
        /// <returns></returns>
        public ListViewItem GetListViewItem()
        {
            return new ListViewItem(new[] 
            {
                ScreenIndex.ToString(),
                Description,
                //PlayList,
                //Playfile,
                //Loopit,
                Active,
                //Priority,
                CustomPos
            }
            );
        }

        /// <summary>
        /// Highlight this pup screen (make it red, select it and bring to front)
        /// or else make it yellow and de-select it
        /// </summary>
        /// <param name="yes"></param>
        public void Highlight(bool yes)
        {
            if (yes)
            {
                Window.SetColors(false, null, null, Color.Red);
                Window.BringToFront();
                Window.IsSelected = true;
            }
            else
            {
                Window.SetColors(false, null, null, Color.Yellow);
                Window.IsSelected = false;
            }
        }

        /// <summary>
        /// highlight the window (or not) depending on its IsSelected property
        /// </summary>
        public void HighlightIfSelected()
        {
            Highlight(Window.IsSelected);
        }
    }


}
