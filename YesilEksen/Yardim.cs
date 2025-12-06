using System;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Yardim : Form
    {
        public Yardim()
        {
            InitializeComponent();
        }

        private void Yardim_Load(object sender, EventArgs e)
        {
            try
            {
                // Form başlığı
                this.Text = "Yeşil Eksen - Kullanım Kılavuzu";
                this.StartPosition = FormStartPosition.CenterScreen;

                // Geri butonu event handler
                button1.Click += BtnGeri_Click;

                // Yardım içeriği
                richTextBox1.Text = GetYardimText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetYardimText()
        {
            return @"YEŞİL EKSEN - KULLANIM KILAVUZU
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔐 GİRİŞ İŞLEMLERİ
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
1. Kullanıcı adı ve parolanızı girin
2. 'Giriş Yap' butonuna tıklayın
3. Rol tipinize göre ilgili dashboard'a yönlendirileceksiniz

Test Kullanıcıları:
• Sanayi Odası Admin: sanayi_admin / 123456
• Ziraat Odası Admin: ziraat_admin / 123456

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🏭 SANAYİ ODASI PANELİ
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📌 Firma Onay Sistemi
1. Sol menüden 'Firma-Onay' seçin
2. Onay bekleyen firmalar listesinden bir firma seçin
3. Firma bilgilerini ve belgelerini inceleyin
4. 'Onayla' veya 'Reddet' butonuna tıklayın
   (Not: Reddetme için neden yazmanız gerekir)

📌 Alım Talebi Yönetimi
1. Sol menüden 'Firma-Taleb' seçin
2. Onay bekleyen talepleri görüntüleyin
3. Talep detaylarını inceleyin
4. Firma ve çiftlik durumlarını kontrol edin
5. Talebi onaylayın veya reddedin

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📊 RAPORLAMA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📌 Genel Rapor
• Tüm firma ve talep verilerini görüntüleyin
• Arz-Talep dengesini takip edin
• Sektörel dağılımları inceleyin
• Verileri Excel'e aktarın

📌 SDK Raporu (Sürdürülebilirlik)
• Geri kazanılan atık miktarını görüntüleyin
• Engellenen CO2 salınımını takip edin
• Ekonomik katkıyı hesaplayın
• Çevresel etki grafiklerini inceleyin
• Raporu Excel'e aktarın

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📤 EXCEL AKTARIMI
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
1. İlgili rapor sayfasına gidin
2. 'Excel'e Aktar' butonuna tıklayın
3. Kayıt konumunu seçin
4. Dosya otomatik olarak oluşturulacaktır

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❓ SIKÇA SORULAN SORULAR
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

S: Şifremi unuttum, ne yapmalıyım?
C: Sistem yöneticinizle iletişime geçin.

S: Bir firmayı yanlışlıkla reddettim, geri alabilir miyim?
C: Hayır, işlemler geri alınamaz. Log kayıtlarını kontrol edebilirsiniz.

S: Raporları hangi formatta aktarabilirim?
C: CSV formatında aktarabilirsiniz. Excel'de açılabilir.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📞 İLETİŞİM
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Teknik Destek: keremisik1010@gmail.com
                kivr.mehmet@gmail.com

© 2025 Yeşil Eksen";
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
