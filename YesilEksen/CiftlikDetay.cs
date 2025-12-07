using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using QRCoder;

namespace YesilEksen
{
    public partial class CiftlikDetay : Form
    {
        public int CiftlikID { get; set; }

        public CiftlikDetay()
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

                // Çiftlik bilgilerini yükle
                if (CiftlikID > 0)
                {
                    LoadCiftlikDetay();
                    LoadUrunListesi();
                    LoadIstatistikler();
                    LoadQRCode();
                }

                // Grafiği yükle
                LoadChart();

                // Label başlıklarını güncelle
                lblgecmisbaslik.Text = "Ürün Listesi";
                grpzirveler.Text = "Üretim Bilgileri";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Çiftlik detaylarını yükler
        /// </summary>
        private void LoadCiftlikDetay()
        {
            try
            {
                string query = @"
                    SELECT 
                        c.Unvan, c.VergiNo, c.Adres,
                        s.SektorAdi, sh.SehirAdi, d.DurumAdi
                    FROM Tbl_Ciftlikler c
                    LEFT JOIN Tbl_Sektorler s ON c.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON c.DurumID = d.DurumID
                    WHERE c.CiftlikID = @ciftlikID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@ciftlikID", CiftlikID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string unvan = row["Unvan"]?.ToString() ?? "Bilinmiyor";
                    string sektor = row["SektorAdi"]?.ToString() ?? "";
                    string sehir = row["SehirAdi"]?.ToString() ?? "";
                    string durum = row["DurumAdi"]?.ToString() ?? "";
                    string vergiNo = row["VergiNo"]?.ToString() ?? "";

                    this.Text = $"Çiftlik Detay - {unvan}";
                    
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
                MessageBox.Show($"Çiftlik detayları yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ürün listesini yükler
        /// </summary>
        private void LoadUrunListesi()
        {
            try
            {
                // DataGridView'i temizle ve yeniden yapılandır
                dataGridView1.Columns.Clear();
                dataGridView1.AutoGenerateColumns = true;

                string query = @"
                    SELECT 
                        u.UrunAdi as 'Ürün Adı',
                        k.KategoriAdi as 'Kategori',
                        u.MiktarTon as 'Miktar (Ton)',
                        d.DurumAdi as 'Durum'
                    FROM Tbl_CiftlikUrunleri u
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID
                    LEFT JOIN Tbl_OnayDurumlari d ON u.DurumID = d.DurumID
                    WHERE u.CiftlikID = @ciftlikID
                    ORDER BY u.MiktarTon DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@ciftlikID", CiftlikID));

                dataGridView1.DataSource = dt;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün listesi yüklenirken hata: {ex.Message}", 
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
                // Toplam ürün sayısı
                object urunResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Tbl_CiftlikUrunleri WHERE CiftlikID = @ciftlikID",
                    new SQLiteParameter("@ciftlikID", CiftlikID));
                int toplamUrun = urunResult != null ? Convert.ToInt32(urunResult) : 0;
                lbltislem.Text = $"Toplam Ürün Çeşidi: {toplamUrun}";

                // Toplam üretim hacmi (tüm ürünler)
                object hacimResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(MiktarTon), 0) FROM Tbl_CiftlikUrunleri WHERE CiftlikID = @ciftlikID",
                    new SQLiteParameter("@ciftlikID", CiftlikID));
                double toplamHacim = hacimResult != null ? Convert.ToDouble(hacimResult) : 0;
                lblthacim.Text = $"Toplam Üretim: {toplamHacim:N2} Ton";

                // En çok üretilen ürün
                object enCokResult = DatabaseHelper.ExecuteScalar(@"
                    SELECT UrunAdi FROM Tbl_CiftlikUrunleri 
                    WHERE CiftlikID = @ciftlikID
                    ORDER BY MiktarTon DESC 
                    LIMIT 1",
                    new SQLiteParameter("@ciftlikID", CiftlikID));
                string enCokUrun = enCokResult?.ToString() ?? "-";
                lblsikurun.Text = $"En Çok Üretilen: {enCokUrun}";

                // Onaylı ürün sayısı
                object onayliResult = DatabaseHelper.ExecuteScalar(@"
                    SELECT COUNT(*) FROM Tbl_CiftlikUrunleri 
                    WHERE CiftlikID = @ciftlikID AND DurumID = 2",
                    new SQLiteParameter("@ciftlikID", CiftlikID));
                int onayliUrun = onayliResult != null ? Convert.ToInt32(onayliResult) : 0;
                lblsikciftlik.Text = $"Onaylı Ürün Sayısı: {onayliUrun}";
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
                chartArea.AxisX.Title = "Ürün";
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.LabelStyle.Angle = -45;
                charthacimgrafik.ChartAreas.Add(chartArea);

                // Başlık ekle
                charthacimgrafik.Titles.Clear();
                charthacimgrafik.Titles.Add("Ürün Bazlı Üretim Miktarları");
                charthacimgrafik.Titles[0].ForeColor = Color.White;
                charthacimgrafik.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
                charthacimgrafik.BackColor = Color.FromArgb(30, 30, 30);

                // Series ekle
                Series series = new Series("Üretim Miktarı");
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.ForestGreen;

                // Veritabanından ürün verilerini çek (tüm ürünler - onaylı olmayanlar dahil)
                if (CiftlikID > 0)
                {
                    string query = @"
                        SELECT 
                            UrunAdi,
                            MiktarTon
                        FROM Tbl_CiftlikUrunleri 
                        WHERE CiftlikID = @ciftlikID
                        ORDER BY MiktarTon DESC
                        LIMIT 6";
                    
                    DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                        new SQLiteParameter("@ciftlikID", CiftlikID));

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string urunAdi = row["UrunAdi"]?.ToString() ?? "";
                            double miktar = Convert.ToDouble(row["MiktarTon"]);
                            DataPoint dp = new DataPoint();
                            dp.SetValueXY(urunAdi, miktar);
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
            Ciftlikler ciftlikler = new Ciftlikler();
            ciftlikler.Show();
            this.Close();
        }

        /// <summary>
        /// Çiftlik için QR kod oluşturur ve PictureBox'a yükler
        /// QR kod okutulduğunda çiftlik bilgileri görünecek
        /// </summary>
        private void LoadQRCode()
        {
            try
            {
                if (CiftlikID <= 0)
                    return;

                // Çiftlik bilgilerini veritabanından çek
                string query = @"
                    SELECT 
                        c.Unvan, c.VergiNo, c.Adres,
                        s.SektorAdi, sh.SehirAdi, d.DurumAdi
                    FROM Tbl_Ciftlikler c
                    LEFT JOIN Tbl_Sektorler s ON c.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON c.DurumID = d.DurumID
                    WHERE c.CiftlikID = @ciftlikID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@ciftlikID", CiftlikID));

                if (dt.Rows.Count == 0)
                    return;

                DataRow row = dt.Rows[0];
                string unvan = row["Unvan"]?.ToString() ?? "Bilinmiyor";
                string vergiNo = row["VergiNo"]?.ToString() ?? "";
                string adres = row["Adres"]?.ToString() ?? "";
                string sektor = row["SektorAdi"]?.ToString() ?? "";
                string sehir = row["SehirAdi"]?.ToString() ?? "";
                string durum = row["DurumAdi"]?.ToString() ?? "";

                // Toplam üretim miktarını hesapla
                object hacimResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(MiktarTon), 0) FROM Tbl_CiftlikUrunleri WHERE CiftlikID = @ciftlikID",
                    new SQLiteParameter("@ciftlikID", CiftlikID));
                double toplamHacim = hacimResult != null ? Convert.ToDouble(hacimResult) : 0;

                // QR kod içeriği - Okunabilir format
                string qrContent = $"=== ÇİFTLİK BİLGİLERİ ===\n\n" +
                    $"Çiftlik Adı: {unvan}\n" +
                    $"Vergi No: {vergiNo}\n" +
                    $"Sektör: {sektor}\n" +
                    $"Şehir: {sehir}\n" +
                    $"Adres: {adres}\n" +
                    $"Durum: {durum}\n" +
                    $"Toplam Üretim: {toplamHacim:N2} Ton\n" +
                    $"\nÇiftlik ID: {CiftlikID}\n" +
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
