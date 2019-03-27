using System;
using System.Windows.Forms;

namespace CustomPos
{
    public partial class B2sForm : Form
    {
        public B2sForm(string message, string buttonText1, bool btn1Enabled, string buttonText2, bool btn2Enabled, string buttonText3, bool btn3Enabled)
        {
            InitializeComponent();
            lblQuestion.Text = message;
            btn1.Text = buttonText1;
            btn1.Enabled = btn1Enabled;
            btn2.Text = buttonText2;
            btn2.Enabled = btn2Enabled;
            btn3.Text = buttonText3;
            btn3.Enabled = btn3Enabled;

        }

        private void btn1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
