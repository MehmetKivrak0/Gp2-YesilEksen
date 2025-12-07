using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using QRCoder;

namespace YesilEksen
{
    public partial class FirmaDetay : Form
    {
        public int FirmaID { get; set; }

        public FirmaDetay()
        {
            InitializeComponent();
        }

        private void FirmaDetay_Load(object sender, EventArgs e)
        {
            try
            {
                // Form boyutunu ayarla
                this.Size = new Size(1280, 750);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Event handler'ları bağla
                geriToolStripMenuItem.Click += BtnGeri_Click;
                yardımToolStripMenuItem.Click += BtnYardim_Click;

                // Firma bilgilerini yükle
                if (FirmaID > 0)
                {
                    LoadFirmaDetay();
                    LoadSatinAlmaGecmisi();
                    LoadIstatistikler();
                    LoadQRCode();
                }

                // Grafiği yükle
                LoadChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Firma detaylarını yükler
        /// </summary>
        private void LoadFirmaDetay()
        {
            try
            {
                string query = @"
                    SELECT 
                        f.Unvan, f.VergiNo, f.Adres,
                        s.SektorAdi, sh.SehirAdi, d.DurumAdi
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    WHERE f.FirmaID = @firmaID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@firmaID", FirmaID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string unvan = row["Unvan"]?.ToString() ?? "Bilinmiyor";
                    string sektor = row["SektorAdi"]?.ToString() ?? "";
                    string sehir = row["SehirAdi"]?.ToString() ?? "";
                    string durum = row["DurumAdi"]?.ToString() ?? "";
                    string vergiNo = row["VergiNo"]?.ToString() ?? "";

                    this.Text = $"Firma Detay - {unvan}";
                    
                    // Label'ları doldur
                    lblfirmaadi.Text = unvan;
                    lblyetkili.Text = $"Sektör: {sektor} | Şehir: {sehir}";
                    lbltel.Text = $"Vergi No: {vergiNo}";
                    lbldurum.Text = $"Durumu: {durum}";

                    // Durum rengini ayarla
                    if (durum == "Onaylandı")
                        lbldurum.ForeColor = Color.Green;
                    else if (durum == "Reddedildi")
                        lbldurum.ForeColor = Color.Red;
                    else
                        lbldurum.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firma detayları yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Satın alma geçmişini yükler
        /// </summary>
        private void LoadSatinAlmaGecmisi()
        {
            try
            {
                // DataGridView'i temizle ve yeniden yapılandır
                dataGridView1.Columns.Clear();
                dataGridView1.AutoGenerateColumns = true;

                string query = @"
                    SELECT 
                        t.TalepTarihi as 'Talep Tarihi',
                        COALESCE(u.UrunAdi, '-') as 'Ürün Adı',
                        t.TalepMiktarTon as 'Miktar (Ton)',
                        COALESCE(c.Unvan, '-') as 'Çiftlik'
                    FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_CiftlikUrunleri u ON t.UrunID = u.UrunID
                    LEFT JOIN Tbl_Ciftlikler c ON t.HedefCiftlikID = c.CiftlikID
                    WHERE t.FirmaID = @firmaID
                    ORDER BY t.TalepTarihi DESC
                    LIMIT 10";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@firmaID", FirmaID));

                dataGridView1.DataSource = dt;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Satın alma geçmişi yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// İstatistikleri yükler
        /// </summary>
        private void LoadIstatistikler()
        {
            try
            {
                // Toplam işlem sayısı
                object islemResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_AlimTalepleri WHERE FirmaID = @firmaID",
                    new SQLiteParameter("@firmaID", FirmaID));
                int toplamIslem = islemResult != null ? Convert.ToInt32(islemResult) : 0;
                lbltislem.Text = $"Toplam İşlem: {toplamIslem}";

                // Toplam hacim (tüm talepler)
                object hacimResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(TalepMiktarTon), 0) FROM Tbl_AlimTalepleri WHERE FirmaID = @firmaID",
                    new SQLiteParameter("@firmaID", FirmaID));
                double toplamHacim = hacimResult != null ? Convert.ToDouble(hacimResult) : 0;
                lblthacim.Text = $"Toplam Hacim: {toplamHacim:N2} Ton";

                // En çok alınan ürün
                object urunResult = DatabaseHelper.ExecuteScalar(@"
                    SELECT u.UrunAdi FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_CiftlikUrunleri u ON t.UrunID = u.UrunID
                    WHERE t.FirmaID = @firmaID 
                    GROUP BY t.UrunID 
                    ORDER BY SUM(t.TalepMiktarTon) DESC 
                    LIMIT 1",
                    new SQLiteParameter("@firmaID", FirmaID));
                string enCokUrun = urunResult?.ToString() ?? "-";
                lblsikurun.Text = $"En Çok Alınan Ürün: {enCokUrun}";

                // En sık çalışılan çiftlik
                object ciftlikResult = DatabaseHelper.ExecuteScalar(@"
                    SELECT c.Unvan FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_Ciftlikler c ON t.HedefCiftlikID = c.CiftlikID
                    WHERE t.FirmaID = @firmaID AND t.HedefCiftlikID IS NOT NULL
                    GROUP BY t.HedefCiftlikID 
                    ORDER BY COUNT(*) DESC 
                    LIMIT 1",
                    new SQLiteParameter("@firmaID", FirmaID));
                string enSikCiftlik = ciftlikResult?.ToString() ?? "-";
                lblsikciftlik.Text = $"En Sık Çalışılan Çiftlik: {enSikCiftlik}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Grafiği yükler
        /// </summary>
        private void LoadChart()
        {
            try
            {
                // Grafiği temizle
                charthacimgrafik.Series.Clear();
                charthacimgrafik.ChartAreas.Clear();

                // ChartArea ekle
                ChartArea chartArea = new ChartArea("AnaAlan");
                chartArea.BackColor = Color.FromArgb(30, 30, 30);
                chartArea.AxisX.MajorGrid.LineColor = Color.DimGray;
                chartArea.AxisY.MajorGrid.LineColor = Color.DimGray;
                chartArea.AxisX.LineColor = Color.White;
                chartArea.AxisY.LineColor = Color.White;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.Title = "Ton";
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisX.Title = "Ay";
                chartArea.AxisX.TitleForeColor = Color.White;
                charthacimgrafik.ChartAreas.Add(chartArea);

                // Başlık ekle
                charthacimgrafik.Titles.Clear();
                charthacimgrafik.Titles.Add("Aylık Alım Hacmi");
                charthacimgrafik.Titles[0].ForeColor = Color.White;
                charthacimgrafik.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
                charthacimgrafik.BackColor = Color.FromArgb(30, 30, 30);

                // Series ekle
                Series series = new Series("Aylık Alım Hacmi");
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.ForestGreen;

                // Veritabanından alım talepleri verilerini çek (tüm talepler)
                if (FirmaID > 0)
                {
                    string query = @"
                        SELECT 
                            strftime('%m', TalepTarihi) as Ay,
                            SUM(TalepMiktarTon) as ToplamMiktar
                        FROM Tbl_AlimTalepleri 
                        WHERE FirmaID = @firmaID
                        GROUP BY strftime('%m', TalepTarihi)
                        ORDER BY Ay";
                    
                    DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                        new SQLiteParameter("@firmaID", FirmaID));

                    string[] aylar = { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", 
                                       "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            int ayNo = Convert.ToInt32(row["Ay"]);
                            double miktar = Convert.ToDouble(row["ToplamMiktar"]);
                            DataPoint dp = new DataPoint();
                            dp.SetValueXY(aylar[ayNo], miktar);
                            dp.Label = miktar.ToString("N1");
                            dp.LabelForeColor = Color.White;
                            series.Points.Add(dp);
                        }
                    }
                    else
                    {
                        // Veri yoksa bilgi mesajı göster
                        DataPoint dp = new DataPoint();
                        dp.SetValueXY("Veri Yok", 0);
                        series.Points.Add(dp);
                    }
                }
                else
                {
                    DataPoint dp = new DataPoint();
                    dp.SetValueXY("Veri Yok", 0);
                    series.Points.Add(dp);
                }

                charthacimgrafik.Series.Add(series);
                charthacimgrafik.Legends.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grafik yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            Firmalar firmalar = new Firmalar();
            firmalar.Show();
            this.Close();
        }

        /// <summary>
        /// Firma için QR kod oluşturur ve PictureBox'a yükler
        /// QR kod okutulduğunda firma bilgileri görünecek
        /// </summary>
        private void LoadQRCode()
        {
            try
            {
                if (FirmaID <= 0)
                    return;

                // Firma bilgilerini veritabanından çek
                string query = @"
                    SELECT 
                        f.Unvan, f.VergiNo, f.Adres,
                        s.SektorAdi, sh.SehirAdi, d.DurumAdi
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    WHERE f.FirmaID = @firmaID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@firmaID", FirmaID));

                if (dt.Rows.Count == 0)
                    return;

                DataRow row = dt.Rows[0];
                string unvan = row["Unvan"]?.ToString() ?? "Bilinmiyor";
                string vergiNo = row["VergiNo"]?.ToString() ?? "";
                string adres = row["Adres"]?.ToString() ?? "";
                string sektor = row["SektorAdi"]?.ToString() ?? "";
                string sehir = row["SehirAdi"]?.ToString() ?? "";
                string durum = row["DurumAdi"]?.ToString() ?? "";

                // QR kod içeriği - Okunabilir format
                string qrContent = $"=== FIRMA BİLGİLERİ ===\n\n" +
                    $"Firma Adı: {unvan}\n" +
                    $"Vergi No: {vergiNo}\n" +
                    $"Sektör: {sektor}\n" +
                    $"Şehir: {sehir}\n" +
                    $"Adres: {adres}\n" +
                    $"Durum: {durum}\n" +
                    $"\nFirma ID: {FirmaID}\n" +
                    $"\nYeşil Eksen - Sürdürülebilir Tarım Platformu";

                // QR kod oluştur
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrData))
                    {
                        // PictureBox boyutuna göre QR kod oluştur
                        int qrSize = Math.Min(picqr.Width, picqr.Height);
                        if (qrSize < 100) qrSize = 200; // Minimum boyut
                        
                        Bitmap qrBitmap = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                        picqr.Image = qrBitmap;
                        picqr.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QR kod oluşturulurken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnYardim_Click(object sender, EventArgs e)
        {
            Yardim yardim = new Yardim();
            yardim.ShowDialog();
        }
    }
}
