using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YesilEksen.Tarım
{
    public partial class ÇitflikÜrünOnay : Form
    {
        public ÇitflikÜrünOnay()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SdkRapor sdkRapor = new SdkRapor();
            sdkRapor.Show();
            this.Hide();
        }

        private void btnsdkrapor_Click(object sender, EventArgs e)
        {
                GenelRapor genelRapor = new GenelRapor();
                genelRapor.Show();
                this.Hide();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {

        }

        private void pnlAltMenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ÇiftlikOnay çiftlikOnay= new ÇiftlikOnay();
            çiftlikOnay.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ÇiftlikOnay çiftlikOnay = new ÇiftlikOnay();
            çiftlikOnay.Show();
            this.Hide();
        }
    }
}
