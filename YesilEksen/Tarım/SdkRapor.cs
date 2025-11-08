using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YesilEksen.Tarım;

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
            Çİftçi_Dasboard çİftçi_Dasboard = new Çİftçi_Dasboard();
            çİftçi_Dasboard.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           ÇiftlikOnay  çiftlikOnay = new ÇiftlikOnay();
            çiftlikOnay.Show();
            this.Hide();

        }

        private void SdkRapor_Load(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            ÇitflikÜrünOnay çiftlikÜrünOnay = new ÇitflikÜrünOnay();
            çiftlikÜrünOnay.Show();
            this.Hide();
        }
    }
}
