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
    public partial class GenelRapor : Form
    {
        public GenelRapor()
        {
            InitializeComponent();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SanayiFirmaOnay sanayiFirmaOnay = new SanayiFirmaOnay();
            sanayiFirmaOnay.Show();
            this.Hide();
        }

        private void GenelRapor_Load(object sender, EventArgs e)
        {

        }

        private void btnsdkrapor_Click(object sender, EventArgs e)
        {
            SdkRapor sdkRapor = new SdkRapor();
            sdkRapor.Show();
            this.Hide();
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
            sanayiAlımTalebi.Show();
            this.Hide();
        }
    }
}
