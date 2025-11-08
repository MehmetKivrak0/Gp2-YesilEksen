using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YesilEksen.Sanayi;

namespace YesilEksen
{
    public partial class SanayiFirmaOnay : Form
    {
        public SanayiFirmaOnay()
        {
            InitializeComponent();
        }

        private void SanayiFirmaOnay_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void btngnrapor_Click(object sender, EventArgs e)
        {
            SanayiGenelRapor sanayiGenelRapor = new SanayiGenelRapor();
            sanayiGenelRapor.Show();
            this.Hide();
        }

        private void btnsdkrapor_Click(object sender, EventArgs e)
        {
            SdkRapor sdkRapor = new SdkRapor();
            sdkRapor.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
            sanayiAlımTalebi.Show();
            this.Hide();
        }
    }
}
