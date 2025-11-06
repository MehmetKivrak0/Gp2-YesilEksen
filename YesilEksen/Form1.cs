using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;

        }

        private void btnDetaygit_Click(object sender, EventArgs e)
        {
            SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
            sanayiFirma.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SdkRapor sdkRapor = new SdkRapor();
            sdkRapor.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void pnlAltMenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnFirmaOnay_Click(object sender, EventArgs e)
        {
            SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
            sanayiFirma.Show();
            this.Hide();
        }

        private void btnAlımTaleb_Click(object sender, EventArgs e)
        {
            SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
            sanayiAlımTalebi.Show();
            this.Hide();

        }

        private void btnFirmataleb_Click(object sender, EventArgs e)
        {
            SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
            sanayiAlımTalebi.Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
