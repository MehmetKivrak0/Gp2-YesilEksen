namespace YesilEksen
{
    partial class Yardim
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Yardim));
            this.button1 = new System.Windows.Forms.Button();
            this.pnlicerik = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.lbl_yardim = new System.Windows.Forms.Label();
            this.pnlicerik.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LightGray;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 401);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(776, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Geri";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // pnlicerik
            // 
            this.pnlicerik.BackColor = System.Drawing.Color.LightGray;
            this.pnlicerik.Controls.Add(this.richTextBox1);
            this.pnlicerik.Controls.Add(this.lbl_yardim);
            this.pnlicerik.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlicerik.Location = new System.Drawing.Point(0, 0);
            this.pnlicerik.Margin = new System.Windows.Forms.Padding(30, 30, 30, 80);
            this.pnlicerik.Name = "pnlicerik";
            this.pnlicerik.Padding = new System.Windows.Forms.Padding(10);
            this.pnlicerik.Size = new System.Drawing.Size(800, 370);
            this.pnlicerik.TabIndex = 1;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(13, 40);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(10);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(774, 317);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // lbl_yardim
            // 
            this.lbl_yardim.AutoSize = true;
            this.lbl_yardim.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_yardim.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lbl_yardim.Location = new System.Drawing.Point(329, 9);
            this.lbl_yardim.Name = "lbl_yardim";
            this.lbl_yardim.Size = new System.Drawing.Size(135, 17);
            this.lbl_yardim.TabIndex = 0;
            this.lbl_yardim.Text = "Kullanım Kılavuzu";
            this.lbl_yardim.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Yardim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.pnlicerik);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Yardim";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Yardim";
            this.Load += new System.EventHandler(this.Yardim_Load);
            this.pnlicerik.ResumeLayout(false);
            this.pnlicerik.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel pnlicerik;
        private System.Windows.Forms.Label lbl_yardim;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}