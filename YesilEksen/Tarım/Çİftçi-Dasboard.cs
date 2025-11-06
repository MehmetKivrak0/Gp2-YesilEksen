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
    public partial class Çİftçi_Dasboard : Form
    {
        public Çİftçi_Dasboard()
        {
            InitializeComponent();
        }

        private void Çİftçi_Dasboard_Load(object sender, EventArgs e)
        {

        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }
    }
}
