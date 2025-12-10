using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Logo'yu düzgün göster
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                // Hoşgeldin mesajını güncelle
                if (!string.IsNullOrEmpty(Session.KullaniciAdi))
                {
                    label1.Text = $"Sanayi Odası - Hoş Geldin {Session.KullaniciAdi}";
                }

                // İstatistikleri yükle
                LoadStatistics();

                // Son aktiviteleri yükle
                LoadRecentActivities();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// İstatistikleri veritabanından yükler
        /// </summary>
        private void LoadStatistics()
        {
            try
            {
                // Onay bekleyen firma sayısı
                object firmaResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_Firmalar WHERE DurumID = 1");
                int bekleyenFirma = firmaResult != null ? Convert.ToInt32(firmaResult) : 0;
                label5.Text = $"({bekleyenFirma})";
                label6.Text = bekleyenFirma > 0 ? "Acil!" : "Temiz";

                // Onay bekleyen alım talebi sayısı
                object talepResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_AlimTalepleri WHERE DurumID = 1");
                int bekleyenTalep = talepResult != null ? Convert.ToInt32(talepResult) : 0;
                label9.Text = $"({bekleyenTalep})";
                label10.Text = bekleyenTalep > 0 ? "Acil!" : "Temiz";

                // Toplam kayıtlı firma
                object toplamFirmaResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_Firmalar");
                int toplamFirma = toplamFirmaResult != null ? Convert.ToInt32(toplamFirmaResult) : 0;
                label14.Text = $": {toplamFirma}";

                // Toplam onaylanmış talep
                object onayliTalepResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_AlimTalepleri WHERE DurumID = 2");
                int onayliTalep = onayliTalepResult != null ? Convert.ToInt32(onayliTalepResult) : 0;
                label15.Text = $": {onayliTalep}";

                // Onaylanmamış talep sayısı
                object onaysizTalepResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_AlimTalepleri WHERE DurumID != 2");
                int onaysizTalep = onaysizTalepResult != null ? Convert.ToInt32(onaysizTalepResult) : 0;
                label16.Text = $": {onaysizTalep}";

                // Bilgilendirme labeli
                label17.Text = "Veriler veritabanından anlık çekilmektedir.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Son aktiviteleri DataGridView'e yükler
        /// </summary>
        private void LoadRecentActivities()
        {
            try
            {
                // Sanayi Odası logları (RolID = 3) veya sistem logları (RolID = 0)
                string query = @"
                    SELECT 
                        LogID as 'ID',
                        Aciklama as 'Açıklama',
                        IslemTarihi as 'İşlem Tarihi'
                    FROM Tbl_IslemLoglari 
                    WHERE RolID = 3 OR RolID = 0
                    ORDER BY IslemTarihi DESC 
                    LIMIT 10";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                // Grid görünümünü düzenle
                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["ID"].Width = 50;
                    dataGridView1.Columns["Açıklama"].Width = 600;
                    dataGridView1.Columns["İşlem Tarihi"].Width = 200;
                }

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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

        private void btnDetaygit_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
                sanayiFirma.Show();
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
                // Sanayi Odası için SDK Rapor
                YesilEksen.SdkRapor sdkRapor = new YesilEksen.SdkRapor();
                sdkRapor.Show();
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
            BtnGenelRapor_Click(sender, e);
        }

        private void BtnGenelRapor_Click(object sender, EventArgs e)
        {
            try
            {
                Sanayi.SanayiGenelRapor genelRapor = new Sanayi.SanayiGenelRapor();
                genelRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pnlAltMenu_Paint(object sender, PaintEventArgs e)
        {
            // Alt menü panel paint eventi
        }

        private void btnFirmaOnay_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
                sanayiFirma.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAlımTaleb_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
                sanayiAlımTalebi.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFirmataleb_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiAlımTalebi sanayiAlımTalebi = new SanayiAlımTalebi();
                sanayiAlımTalebi.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Ana sayfaya git
            LoadStatistics();
            LoadRecentActivities();
        }

        private void button3_Click(object sender, EventArgs e)
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

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                Firmalar firmalar = new Firmalar();
                firmalar.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firmalar sayfası açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Form kapatıldığında temizlik
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing && Session.KullaniciID > 0)
            {
                // Sadece eğer session aktifse ve ana pencere kapatılıyorsa
                // Login'e değil, direkt çıkış yapıyorsa uygulamayı kapat
            }
        }
    }
}
