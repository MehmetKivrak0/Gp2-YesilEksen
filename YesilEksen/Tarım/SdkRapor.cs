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
    public partial class SdkRapor : Form
    {
        public SdkRapor()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            GenelRapor genelRapor = new GenelRapor();
            genelRapor.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           SanayiFirmaOnay sanayiFirmaOnay = new SanayiFirmaOnay();
            sanayiFirmaOnay.Show();
            this.Hide();
        }
    }
}
