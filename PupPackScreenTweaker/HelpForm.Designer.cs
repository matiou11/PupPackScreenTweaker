namespace CustomPos
{
    partial class HelpForm
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
            this.rtxt = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtxt
            // 
            this.rtxt.Location = new System.Drawing.Point(12, 12);
            this.rtxt.Name = "rtxt";
            this.rtxt.Size = new System.Drawing.Size(317, 96);
            this.rtxt.TabIndex = 0;
            this.rtxt.Text = "";
            this.rtxt.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtxt_LinkClicked);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(264, 116);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(65, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 152);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.rtxt);
            this.Name = "HelpForm";
            this.Text = "Help/About";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxt;
        private System.Windows.Forms.Button button1;
    }
}