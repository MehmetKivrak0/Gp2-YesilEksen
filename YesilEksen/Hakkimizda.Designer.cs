namespace YesilEksen
{
    partial class Hakkimizda
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Hakkimizda));
            this.tableAnaLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_hakkimizda = new System.Windows.Forms.Label();
            this.tableprofiller = new System.Windows.Forms.TableLayoutPanel();
            this.picmehmet = new System.Windows.Forms.PictureBox();
            this.pickerem = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnGeri = new System.Windows.Forms.Button();
            this.tableAnaLayout.SuspendLayout();
            this.tableprofiller.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picmehmet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pickerem)).BeginInit();
            this.SuspendLayout();
            // 
            // tableAnaLayout
            // 
            this.tableAnaLayout.ColumnCount = 1;
            this.tableAnaLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableAnaLayout.Controls.Add(this.lbl_hakkimizda, 0, 0);
            this.tableAnaLayout.Controls.Add(this.tableprofiller, 0, 1);
            this.tableAnaLayout.Controls.Add(this.richTextBox1, 0, 2);
            this.tableAnaLayout.Controls.Add(this.btnGeri, 0, 3);
            this.tableAnaLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableAnaLayout.Location = new System.Drawing.Point(0, 0);
            this.tableAnaLayout.Name = "tableAnaLayout";
            this.tableAnaLayout.RowCount = 4;
            this.tableAnaLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableAnaLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableAnaLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 266F));
            this.tableAnaLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableAnaLayout.Size = new System.Drawing.Size(800, 450);
            this.tableAnaLayout.TabIndex = 0;
            // 
            // lbl_hakkimizda
            // 
            this.lbl_hakkimizda.AutoSize = true;
            this.lbl_hakkimizda.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_hakkimizda.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_hakkimizda.Location = new System.Drawing.Point(3, 0);
            this.lbl_hakkimizda.Name = "lbl_hakkimizda";
            this.lbl_hakkimizda.Padding = new System.Windows.Forms.Padding(10);
            this.lbl_hakkimizda.Size = new System.Drawing.Size(794, 33);
            this.lbl_hakkimizda.TabIndex = 0;
            this.lbl_hakkimizda.Text = "Hakkımızda";
            this.lbl_hakkimizda.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_hakkimizda.Click += new System.EventHandler(this.label1_Click);
            // 
            // tableprofiller
            // 
            this.tableprofiller.ColumnCount = 2;
            this.tableprofiller.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableprofiller.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableprofiller.Controls.Add(this.picmehmet, 0, 0);
            this.tableprofiller.Controls.Add(this.pickerem, 1, 0);
            this.tableprofiller.Controls.Add(this.label1, 0, 1);
            this.tableprofiller.Controls.Add(this.label2, 1, 1);
            this.tableprofiller.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableprofiller.Location = new System.Drawing.Point(3, 36);
            this.tableprofiller.Name = "tableprofiller";
            this.tableprofiller.RowCount = 2;
            this.tableprofiller.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.33334F));
            this.tableprofiller.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.66667F));
            this.tableprofiller.Size = new System.Drawing.Size(794, 100);
            this.tableprofiller.TabIndex = 1;
            // 
            // picmehmet
            // 
            this.picmehmet.BackColor = System.Drawing.Color.DarkGray;
            this.picmehmet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picmehmet.Image = global::YesilEksen.Properties.Resources.mehmet;
            this.picmehmet.Location = new System.Drawing.Point(3, 3);
            this.picmehmet.Name = "picmehmet";
            this.picmehmet.Size = new System.Drawing.Size(391, 81);
            this.picmehmet.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picmehmet.TabIndex = 0;
            this.picmehmet.TabStop = false;
            // 
            // pickerem
            // 
            this.pickerem.BackColor = System.Drawing.Color.DarkGray;
            this.pickerem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pickerem.Image = global::YesilEksen.Properties.Resources.kerem;
            this.pickerem.Location = new System.Drawing.Point(400, 3);
            this.pickerem.Name = "pickerem";
            this.pickerem.Size = new System.Drawing.Size(391, 81);
            this.pickerem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pickerem.TabIndex = 1;
            this.pickerem.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.DarkGray;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mehmet Kıvrak";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.DarkGray;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(400, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(391, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Kerem Işık";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.richTextBox1.Location = new System.Drawing.Point(19, 149);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(19, 10, 10, 10);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(771, 246);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // btnGeri
            // 
            this.btnGeri.BackColor = System.Drawing.Color.LightGray;
            this.btnGeri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGeri.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGeri.Location = new System.Drawing.Point(10, 415);
            this.btnGeri.Margin = new System.Windows.Forms.Padding(10);
            this.btnGeri.Name = "btnGeri";
            this.btnGeri.Size = new System.Drawing.Size(780, 25);
            this.btnGeri.TabIndex = 3;
            this.btnGeri.Text = "Geri";
            this.btnGeri.UseVisualStyleBackColor = false;
            // 
            // Hakkimizda
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.tableAnaLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Hakkimizda";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hakkimizda";
            this.Load += new System.EventHandler(this.Hakkimizda_Load);
            this.tableAnaLayout.ResumeLayout(false);
            this.tableAnaLayout.PerformLayout();
            this.tableprofiller.ResumeLayout(false);
            this.tableprofiller.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picmehmet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pickerem)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableAnaLayout;
        private System.Windows.Forms.Label lbl_hakkimizda;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnGeri;
        private System.Windows.Forms.TableLayoutPanel tableprofiller;
        private System.Windows.Forms.PictureBox picmehmet;
        private System.Windows.Forms.PictureBox pickerem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}