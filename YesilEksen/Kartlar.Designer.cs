namespace YesilEksen
{
    partial class Kartlar
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblbaslik = new System.Windows.Forms.Label();
            this.lblsektor = new System.Windows.Forms.Label();
            this.lblsehir = new System.Windows.Forms.Label();
            this.lbldurum = new System.Windows.Forms.Label();
            this.btndetay = new System.Windows.Forms.Button();
            this.piclogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.piclogo)).BeginInit();
            this.SuspendLayout();
            // 
            // lblbaslik
            // 
            this.lblbaslik.AutoSize = true;
            this.lblbaslik.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblbaslik.Location = new System.Drawing.Point(11, 72);
            this.lblbaslik.Name = "lblbaslik";
            this.lblbaslik.Size = new System.Drawing.Size(45, 13);
            this.lblbaslik.TabIndex = 1;
            this.lblbaslik.Text = "Firma :";
            // 
            // lblsektor
            // 
            this.lblsektor.AutoSize = true;
            this.lblsektor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblsektor.Location = new System.Drawing.Point(4, 104);
            this.lblsektor.Name = "lblsektor";
            this.lblsektor.Size = new System.Drawing.Size(52, 13);
            this.lblsektor.TabIndex = 2;
            this.lblsektor.Text = "Sektör :";
            // 
            // lblsehir
            // 
            this.lblsehir.AutoSize = true;
            this.lblsehir.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblsehir.Location = new System.Drawing.Point(12, 137);
            this.lblsehir.Name = "lblsehir";
            this.lblsehir.Size = new System.Drawing.Size(44, 13);
            this.lblsehir.TabIndex = 3;
            this.lblsehir.Text = "Şehir :";
            // 
            // lbldurum
            // 
            this.lbldurum.AutoSize = true;
            this.lbldurum.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbldurum.Location = new System.Drawing.Point(25, 180);
            this.lbldurum.Name = "lbldurum";
            this.lbldurum.Size = new System.Drawing.Size(58, 13);
            this.lbldurum.TabIndex = 4;
            this.lbldurum.Text = "Durumu :";
            this.lbldurum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btndetay
            // 
            this.btndetay.Location = new System.Drawing.Point(53, 205);
            this.btndetay.Name = "btndetay";
            this.btndetay.Size = new System.Drawing.Size(86, 29);
            this.btndetay.TabIndex = 5;
            this.btndetay.Text = "Detay";
            this.btndetay.UseVisualStyleBackColor = true;
            this.btndetay.Click += new System.EventHandler(this.btndetay_Click);
            // 
            // piclogo
            // 
            this.piclogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.piclogo.Location = new System.Drawing.Point(0, 0);
            this.piclogo.Name = "piclogo";
            this.piclogo.Size = new System.Drawing.Size(198, 57);
            this.piclogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.piclogo.TabIndex = 0;
            this.piclogo.TabStop = false;
            // 
            // Kartlar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btndetay);
            this.Controls.Add(this.lbldurum);
            this.Controls.Add(this.lblsehir);
            this.Controls.Add(this.lblsektor);
            this.Controls.Add(this.lblbaslik);
            this.Controls.Add(this.piclogo);
            this.Name = "Kartlar";
            this.Size = new System.Drawing.Size(198, 248);
            ((System.ComponentModel.ISupportInitialize)(this.piclogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox piclogo;
        private System.Windows.Forms.Label lblbaslik;
        private System.Windows.Forms.Label lblsektor;
        private System.Windows.Forms.Label lblsehir;
        private System.Windows.Forms.Label lbldurum;
        private System.Windows.Forms.Button btndetay;
    }
}
