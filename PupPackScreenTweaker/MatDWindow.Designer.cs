namespace PupPackScreenTweaker
{
    partial class MatDWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCaption = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.BackColor = System.Drawing.Color.White;
            this.lblCaption.Location = new System.Drawing.Point(27, 3);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(55, 13);
            this.lblCaption.TabIndex = 0;
            this.lblCaption.Text = "MyScreen";
            this.lblCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblCaption_MouseDown);
            this.lblCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblCaption_MouseMove);
            this.lblCaption.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblCaption_MouseUp);
            // 
            // MatDWindow
            // 
            this.BackColor = System.Drawing.Color.Wheat;
            this.ClientSize = new System.Drawing.Size(400, 100);
            this.Controls.Add(this.lblCaption);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MatDWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Lime;
            this.Load += new System.EventHandler(this.MatDWindow_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MatDWindow_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MatDWindow_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MatDWindow_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MatDWindow_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MatDWindow_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCaption;
    }
}