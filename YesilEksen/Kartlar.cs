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
    public partial class Kartlar : UserControl
    {
        public Kartlar()
        {
            InitializeComponent();
        }
        // Dışarıdan Başlığı (Firma ve Çiftlik) Adını ayarlamak için
        public string Baslik
        {
            get { return lblbaslik.Text; }
            set { lblbaslik.Text = "Firma Adı: " + value; }
        }

        // Sektörü ayarlamak için
        public string Sektor
        {
            get { return lblsektor.Text; }
            set { lblsektor.Text = "Sektör: " + value; }
        }

        // Şehri ayarlamak için
        public string Sehir
        {
            get { return lblsehir.Text; }
            set { lblsehir.Text = "Şehir: " + value; }
        }

        // Logoyu ayarlamak için
        public Image Logo
        {
            get { return piclogo.Image; }
            set { piclogo.Image = value; }
        }

        // Durumu ayarlamak için (renk değişimi ile)
        public string Durum
        {
            set
            {
                lbldurum.Text = "Durumu: " + value;
                if (value == "Onaylandı")
                {
                    lbldurum.BackColor = Color.Green; // Onaylıysa Yeşil
                }
                else
                {
                    lbldurum.BackColor = Color.Red; // Onaylı değilse Kırmızı
                }
            }
        }

        private void btndetay_Click(object sender, EventArgs e)
        {
            Ciftlikler ciftlikler = new Ciftlikler();
            ciftlikler.Show();
            this.Hide();
        }
    }
}
