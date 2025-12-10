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
        // Detay butonuna tıklandığında tetiklenecek event
        public event EventHandler DetayClick;

        public Kartlar()
        {
            InitializeComponent();
        }

        // Başlık etiketi (Firma Adı veya Çiftlik Adı)
        private string baslikEtiketi = "Firma Adı: ";

        // Başlık etiketini ayarlamak için
        public string BaslikEtiketi
        {
            get { return baslikEtiketi; }
            set { baslikEtiketi = value; }
        }

        // Dışarıdan Başlığı (Firma ve Çiftlik) Adını ayarlamak için
        public string Baslik
        {
            get { return lblbaslik.Text.Replace(baslikEtiketi, ""); }
            set { lblbaslik.Text = baslikEtiketi + value; }
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
                    lbldurum.BackColor = Color.Green;
                    lbldurum.ForeColor = Color.White;
                }
                else if (value == "Reddedildi")
                {
                    lbldurum.BackColor = Color.Red;
                    lbldurum.ForeColor = Color.White;
                }
                else
                {
                    lbldurum.BackColor = Color.Orange;
                    lbldurum.ForeColor = Color.Black;
                }
            }
        }

        // Detay butonunu gizlemek/göstermek için
        public bool DetayButonuGoster
        {
            get { return btndetay.Visible; }
            set { btndetay.Visible = value; }
        }

        private void btndetay_Click(object sender, EventArgs e)
        {
            // Event'i tetikle - dışarıdan dinlenecek
            DetayClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
