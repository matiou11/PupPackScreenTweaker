using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PupPackScreenTweaker
{
    /// <summary>
    /// Tools to manipulate Pup screens
    /// </summary>
    public static class PupTools
    {
        /// <summary>
        /// guess where the PinUpPlayer.ini file is... or ask the user
        /// </summary>
        /// <returns></returns>
        public static string FindPuPIniFile()
        {
            string[] tests = { ".\\PinUpPlayer.ini", "..\\PinUpPlayer.ini", "c:\\PinUpSystem\\PinUpPlayer.ini", "c:\\Pinball\\PinUpSystem\\PinUpPlayer.ini", "d:\\PinUpSystem\\PinUpPlayer.ini"  };
            foreach (string test in tests) if (File.Exists(test)) return test;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ini file|*.ini";
            openFileDialog1.Title = "Please show me where your PinUpPlayer.ini is... (it won't be modified)";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            else
            {
                // give up
                return "";
            }
        }

        public static string GetScreenPupHeaders()
        {
            return "ScreenNum,ScreenDes,PlayList,PlayFile,Loopit,Active,Priority,CustomPos";
        }

        /// <summary>
        /// build a collection of pup screens, read from a "screens.pup" file
        /// screens.pup format:
        ///  - csv file
        ///  - 1 line of header (ScreenNum,ScreenDes,PlayList,PlayFile,Loopit,Active,Priority,CustomPos)
        ///  - 1 line per screen (usually 10)
        ///  - Example of CustomPos field: "2,0,23.2,100,49.83" 
        ///      - Screen ref index
        ///      - X position of screen (%) relatively to ref screen
        ///      - Y position of screen (%) relatively to ref screen
        ///      - Width position of screen (%) relatively to ref screen
        ///      - Height position of screen (%) relatively to ref screen
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="transparentByDefault"></param>
        /// <param name="refScreens"></param>
        /// <returns></returns>
        public static PupScreens GetPupScreensFromPupFile(string fileName, bool transparentByDefault, List<PupScreen> refScreens)
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            int lineIndex = 0;
            PupScreens pupScreens = new PupScreens();
            foreach (string line in lines)
            {
                if (lineIndex > 0 && line.Trim() != "")
                {
                    PupScreen pupScreen = new PupScreen(transparentByDefault, Color.Yellow, refScreens);
                    try
                    {
                        pupScreen.LoadFromCsv(line);
                        pupScreen.CalculateRealPos();
                        pupScreens.Add(pupScreen);
                    }
                    catch
                    {
                        return null;
                    }
                }
                lineIndex++;
            }
            return pupScreens;
        }

        public static PupScreens GetPupScreenFromIniFile(string fileName, bool transparentByDefault)
        {
            PupScreens pupScreens = new PupScreens();
            try
            {
                IniManager ini = new IniManager(fileName);
                for (int t = 0; t <= 10; t++)
                {
                    PupScreen pupScreen = new PupScreen(transparentByDefault, null, null);
                    pupScreen.ScreenIndex = t;
                    pupScreen.X = ini.ReadInt("ScreenXPos", "INFO" + (t == 0 ? "" : t.ToString()));
                    pupScreen.Y = ini.ReadInt("ScreenYPos", "INFO" + (t == 0 ? "" : t.ToString()));
                    pupScreen.W = ini.ReadInt("ScreenWidth", "INFO" + (t == 0 ? "" : t.ToString()));
                    pupScreen.H = ini.ReadInt("ScreenHeight", "INFO" + (t == 0 ? "" : t.ToString()));
                    if (pupScreen.X == -1) throw(null);
                    pupScreen.Window.Visible = false;
                    pupScreens.Add(pupScreen);
                }
                if (pupScreens.Count == 0) return null;
            }
            catch
            {
                return null;
            }
            return pupScreens;
        }

        public static string GetSoftwareName()
        {
            return GetSoftwareName(true);
        }

        public static string GetSoftwareName(bool withVersion)
        {
            string softwareName;
            softwareName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            if (withVersion)
            {
                softwareName += " v" + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.Major;
                softwareName += "." + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.Minor;
            }
            return softwareName;
        }

        public static string[] GetActiveModeList()
        {
            string[] list = { "off", "show", "ForceOn", "ForcePop", "ForcePopBack", "ForceBack", "MusicOnly","JavaScript" };
            return list;
        }

        public static string[] GetPlaylists(string folder)
        {
            List<string> lst = new List<string>();
            //return Directory.GetDirectories(folder);
            lst.Add("");
            foreach (string str in Directory.GetDirectories(folder))
            {
                lst.Add(new DirectoryInfo(str).Name);
            }
            return lst.ToArray();
        }

        /// <summary>
        /// Split a csv line into an array of string.
        /// Handles comma which are in string between quotes.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string[] SplitCsv(string line)
        {
            List<string> result = new List<string>();
            StringBuilder currentStr = new StringBuilder("");
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++) // For each character
            {
                if (line[i] == '\"') // Quotes are closing or opening
                    inQuotes = !inQuotes;
                else if (line[i] == ',') // Comma
                {
                    if (!inQuotes) // If not in quotes, end of current string, add it to result
                    {
                        result.Add(currentStr.ToString());
                        currentStr.Clear();
                    }
                    else
                        currentStr.Append(line[i]); // If in quotes, just add it 
                }
                else // Add any other character to current string
                    currentStr.Append(line[i]);
            }
            result.Add(currentStr.ToString());
            return result.ToArray(); // Return array of all strings
        }

        public static void enumScreens()
        {
            string msg = "";
            int monId = 1;
            foreach (Screen screen in Screen.AllScreens)
            {
                string str = String.Format("Monitor {0}: {1} x {2} @ {3},{4}\n", monId, screen.Bounds.Width,
                    screen.Bounds.Height, screen.Bounds.X, screen.Bounds.Y);
                msg += str;
                monId++;
            }

            MessageBox.Show(msg, "EnumDisp");
        }
    }
}
