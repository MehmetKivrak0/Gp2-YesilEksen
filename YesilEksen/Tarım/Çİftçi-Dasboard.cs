using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
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
            try
            {
                // Hoşgeldin mesajını güncelle
                if (!string.IsNullOrEmpty(Session.KullaniciAdi))
                {
                    label1.Text = $"Ziraat Odası - Hoş Geldin {Session.KullaniciAdi}";
                }

                // İstatistikleri yükle
                LoadStatistics();

                // Son aktiviteleri yükle
                LoadRecentActivities();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                // Onay bekleyen çiftlik sayısı
                object ciftlikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_Ciftlikler WHERE DurumID = 1");
                int bekleyenCiftlik = ciftlikResult != null ? Convert.ToInt32(ciftlikResult) : 0;
                label5.Text = $"({bekleyenCiftlik})";
                label6.Text = bekleyenCiftlik > 0 ? "Acil!" : "Temiz";

                // Onay bekleyen ürün sayısı
                object urunResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_CiftlikUrunleri WHERE DurumID = 1");
                int bekleyenUrun = urunResult != null ? Convert.ToInt32(urunResult) : 0;
                label9.Text = $"({bekleyenUrun})";
                label10.Text = bekleyenUrun > 0 ? "Acil!" : "Temiz";

                // Toplam kayıtlı çiftlik
                object toplamCiftlikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_Ciftlikler");
                int toplamCiftlik = toplamCiftlikResult != null ? Convert.ToInt32(toplamCiftlikResult) : 0;
                label21.Text = $": {toplamCiftlik}";
                label24.Text = "Toplam Kayıtlı Çiftlik";

                // Toplam onaylanmış ürün
                object onayliUrunResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_CiftlikUrunleri WHERE DurumID = 2");
                int onayliUrun = onayliUrunResult != null ? Convert.ToInt32(onayliUrunResult) : 0;
                label20.Text = $": {onayliUrun}";
                label23.Text = "Toplam Onaylı Ürün";

                // Onaylanmamış ürün sayısı
                object onaysizUrunResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_CiftlikUrunleri WHERE DurumID != 2");
                int onaysizUrun = onaysizUrunResult != null ? Convert.ToInt32(onaysizUrunResult) : 0;
                label19.Text = $": {onaysizUrun}";
                label22.Text = "Onaysız Ürün Sayısı";

                // Bilgilendirme labeli
                label18.Text = "Veriler veritabanından anlık çekilmektedir.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecentActivities()
        {
            try
            {
                // Ziraat Odası logları (RolID = 4) veya sistem logları (RolID = 0)
                string query = @"
                    SELECT 
                        LogID as 'ID',
                        Aciklama as 'Açıklama',
                        IslemTarihi as 'İşlem Tarihi'
                    FROM Tbl_IslemLoglari 
                    WHERE RolID = 4 OR RolID = 0
                    ORDER BY IslemTarihi DESC 
                    LIMIT 10";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Aktiviteler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ÇiftlikOnay ciftlikOnay = new ÇiftlikOnay();
                ciftlikOnay.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ÇitflikÜrünOnay urunOnay = new ÇitflikÜrünOnay();
                urunOnay.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnsdkrapor_Click(object sender, EventArgs e)
        {
            try
            {
                Tarım.GenelRapor genelRapor = new Tarım.GenelRapor();
                genelRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                YesilEksen.Tarım.SdkRapor sdkRapor = new YesilEksen.Tarım.SdkRapor();
                sdkRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Yardim yardim = new Yardim();
                yardim.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yardım açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", 
                    "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DatabaseHelper.LogIslem($"{Session.KullaniciAdi} çıkış yaptı");
                    Session.Clear();
                    Login.ShowLoginForm();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çıkış yapılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                Ciftlikler ciftlikler = new Ciftlikler();
                ciftlikler.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çiftlikler sayfası açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDetaygit_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        private void btnAlımTaleb_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                Urunler urunler = new Urunler();
                urunler.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler sayfası açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
