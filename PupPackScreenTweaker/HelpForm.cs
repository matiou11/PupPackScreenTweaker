using System;
using System.Windows.Forms;

namespace CustomPos
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            string help = "";
            help += PupTools.GetSoftwareName(true) + Environment.NewLine;
            help += "an accessory software for PuP Pack screen tweaking." + Environment.NewLine;
            help += "Mat D. 03/2019" + Environment.NewLine;
            help += Environment.NewLine;
            help += "https://github.com/matd11/PupPackScreenTweaker";
            rtxt.Text = help;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rtxt_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
