using System;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Hakkimizda : Form
    {
        public Hakkimizda()
        {
            InitializeComponent();
        }

        private void Hakkimizda_Load(object sender, EventArgs e)
        {
            try
            {
                // Form başlığı
                this.Text = "Yeşil Eksen - Hakkımızda";
                this.StartPosition = FormStartPosition.CenterScreen;

                // Geri butonu event handler
                btnGeri.Click += BtnGeri_Click;

                // Hakkımızda içeriği
                richTextBox1.Text = GetHakkimizdaText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetHakkimizdaText()
        {
            return @"YEŞİL EKSEN PROJESİ
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Biz, 'Yeşil Eksen' projemizle, tarımsal ve endüstriyel atık yönetim sürecini denetleyen ve yöneten bir admin platformu oluşturmayı amaçlıyoruz. 

Projemiz, bu sistemin arkaplanda güvenilirliğini sağlayan bir yönetim modelidir.

📋 TEMEL HEDEFİMİZ
• Sisteme başvurmuş olan Çiftliklerin onay süreçlerini yönetmek
• Firmaların ve Ürünlerinin onay süreçlerini yönetmek
• Atık ticaretinin sürdürülebilirlik etkisini ölçmek

🌿 VİZYONUMUZ
Atık yönetimi gibi kritik bir konuda 'güven' faktörünü merkeze almak. Platformun güvenilirliği, ancak 'Sanayi Odası' ve 'Ziraat Odası' gibi kurumlardaki yetkili kişilerin kullanacağı bir denetim aracıyla sağlanabilir.

📊 SÜRDÜRÜLEBİLİRLİK RAPORLARI
• Geri Kazanılan Atık (Ton)
• Engellenen CO2 Salınımı (Ton)
• Ekonomiye Kazandırılan Değer (TL)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📚 DERS BİLGİSİ
BİL 2111 ve BİL 2135 - Görsel Programlama 2
2025-2026 Güz Dönemi Dönem Sonu Projesi

🏫 KURUM
Manisa Celal Bayar Üniversitesi
Manisa Teknik Bilimler Meslek Yüksekokulu
Bilgisayar Programcılığı

👨‍🏫 DANIŞMAN
Doç. Dr. Barış Çukurbaşı

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

© 2025 Yeşil Eksen - Tüm Hakları Saklıdır";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Label click eventi
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
