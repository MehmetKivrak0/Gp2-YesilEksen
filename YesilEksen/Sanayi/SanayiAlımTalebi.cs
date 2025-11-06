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
    public partial class SanayiAlımTalebi : Form
    {
        public SanayiAlımTalebi()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;

        }

        private void btnanasayfa_Click(object sender, EventArgs e)
        {
           Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
            sanayiFirma.Show();
            this.Hide();
        }
    }
}
