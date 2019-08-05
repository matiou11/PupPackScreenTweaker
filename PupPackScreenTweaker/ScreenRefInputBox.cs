using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PupPackScreenTweaker
{
    public partial class ScreenRefInputBox : Form
    {
        public int SelectedRef { get; set; }

        public ScreenRefInputBox(List<string> existingRefScreens, int maxItems)
        {
            InitializeComponent();
            cboRefScreen.Items.Clear();
            SelectedRef = -1;
            for (int i = 1; i < maxItems; i++)
            {
                if (!existingRefScreens.Contains(i.ToString())) cboRefScreen.Items.Add(i.ToString());
            }
        }

        private void ScreenRefInputBox_Load(object sender, EventArgs e)
        {
            btnOk.DialogResult = DialogResult.OK;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int val = -1;
            if (Int32.TryParse(cboRefScreen.Text, out val)) SelectedRef = val;
        }
    }
}
