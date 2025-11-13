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
    public partial class Ciftlikler : Form
    {
        public Ciftlikler()
        {
            InitializeComponent();
        }

        private void FormFirmalar_Load(object sender, EventArgs e)
        {
            // Normalde bu veriler veritabanından çekilecek
            // Biz şimdilik 10 tane test kartı oluşturalım

            flpkartlar.Controls.Clear(); // Önce paneli temizle

            for (int i = 1; i <= 10; i++)
            {
                // 1. Yeni bir KartKontrol oluştur
                Kartlar yeniKart = new Kartlar();

                // 2. Aşama 1'de yazdığımız özellikleri doldur
                yeniKart.Baslik = "Test Firması " + i;
                yeniKart.Sektor = "Bilişim";
                yeniKart.Sehir = "Manisa";
                yeniKart.Durum = (i % 2 == 0) ? "Onaylandı" : "Onay Bekliyor";
                // yeniKart.Logo = Image.FromFile("default_logo.png"); // Bir logo yüklersin

                // 3. Kartı FlowLayoutPanel'a ekle
                flpkartlar.Controls.Add(yeniKart);
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
