using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace YesilEksen.Tarım
{
    public partial class GenelRapor : Form
    {
        private DataTable reportData;

        public GenelRapor()
        {
            InitializeComponent();
        }

        private void GenelRapor_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
            try
            {
                // Başlığı güncelle
                label1.Text = "Ziraat Odası - Genel Rapor";

                // GroupBox başlıklarını güncelle
                groupBox1.Text = "Çiftlik ve Ürün Verileri";
                groupBox2.Text = "Özet İstatistikler";
                groupBox3.Text = "Grafiksel Analiz";

                // Label'ları güncelle
                label2.Text = "Toplam Çiftlik:";
                label3.Text = "";
                label4.Text = "Toplam Ürün:";
                label5.Text = "Onaylı Miktar:";

                // Event handler'ları bağla
                button2.Click += BtnExcelExport_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                btnYardım.Click += BtnYardim_Click;

                // Verileri yükle
                LoadReportData();
                LoadCharts();
                CalculateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Rapor verilerini yükler
        /// </summary>
        private void LoadReportData()
        {
            try
            {
                string query = @"
                    SELECT 
                        c.CiftlikID as 'ID',
                        c.Unvan as 'Çiftlik Ünvanı',
                        s.SektorAdi as 'Sektör',
                        sh.SehirAdi as 'Şehir',
                        d.DurumAdi as 'Durum',
                        (SELECT COUNT(*) FROM Tbl_CiftlikUrunleri WHERE CiftlikID = c.CiftlikID) as 'Ürün Sayısı',
                        (SELECT COALESCE(SUM(MiktarTon), 0) FROM Tbl_CiftlikUrunleri WHERE CiftlikID = c.CiftlikID AND DurumID = 2) as 'Onaylı (Ton)'
                    FROM Tbl_Ciftlikler c
                    LEFT JOIN Tbl_Sektorler s ON c.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON c.DurumID = d.DurumID
                    ORDER BY c.Unvan";

                reportData = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = reportData;

                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rapor verileri yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Grafikleri yükler
        /// </summary>
        private void LoadCharts()
        {
            try
            {
                // ========== Chart1 - Pasta Grafik (Şehirlere Göre Çiftlik Dağılımı) ==========
                chart1.Series.Clear();
                chart1.ChartAreas.Clear();
                chart1.Legends.Clear();
                chart1.Titles.Clear();

                ChartArea pieArea = new ChartArea("PieArea");
                pieArea.BackColor = Color.White;
                chart1.ChartAreas.Add(pieArea);

                Legend pieLegend = new Legend("PieLegend");
                pieLegend.Docking = Docking.Bottom;
                chart1.Legends.Add(pieLegend);

                chart1.Titles.Add("Şehirlere Göre Çiftlik Dağılımı");
                chart1.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series pieSeries = new Series("SehirDagilimi");
                pieSeries.ChartType = SeriesChartType.Pie;
                pieSeries.ChartArea = "PieArea";
                pieSeries.Legend = "PieLegend";

                // Şehire göre çiftlik sayılarını al
                string sehirQuery = @"
                    SELECT sh.SehirAdi, COUNT(*) as Sayi 
                    FROM Tbl_Ciftlikler c 
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID 
                    GROUP BY sh.SehirAdi";
                DataTable sehirData = DatabaseHelper.ExecuteQuery(sehirQuery);

                Color[] colors = { Color.FromArgb(76, 175, 80), Color.FromArgb(33, 150, 243), 
                                   Color.FromArgb(255, 152, 0), Color.FromArgb(156, 39, 176), 
                                   Color.FromArgb(244, 67, 54), Color.FromArgb(0, 188, 212) };
                int colorIndex = 0;

                foreach (DataRow row in sehirData.Rows)
                {
                    DataPoint dp = new DataPoint();
                    string sehir = row["SehirAdi"]?.ToString() ?? "Bilinmiyor";
                    int sayi = Convert.ToInt32(row["Sayi"]);
                    dp.SetValueXY(sehir, sayi);
                    dp.Color = colors[colorIndex % colors.Length];
                    dp.LegendText = $"{sehir} ({sayi})";
                    dp.Label = sayi.ToString();
                    pieSeries.Points.Add(dp);
                    colorIndex++;
                }

                chart1.Series.Add(pieSeries);

                // ========== Chart2 - Çubuk Grafik (Ürün Kategorileri) ==========
                chart2.Series.Clear();
                chart2.ChartAreas.Clear();
                chart2.Legends.Clear();
                chart2.Titles.Clear();

                ChartArea barArea = new ChartArea("BarArea");
                barArea.BackColor = Color.White;
                barArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                barArea.AxisY.MajorGrid.LineColor = Color.LightGray;
                barArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
                barArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
                barArea.AxisY.Title = "Miktar (Ton)";
                barArea.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                chart2.ChartAreas.Add(barArea);

                chart2.Titles.Add("Ürün Kategorilerine Göre Miktar");
                chart2.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series barSeries = new Series("UrunMiktar");
                barSeries.ChartType = SeriesChartType.Column;
                barSeries.ChartArea = "BarArea";
                barSeries.Color = Color.FromArgb(76, 175, 80);

                // Ürün kategorilerine göre toplam miktar
                string urunQuery = @"
                    SELECT k.KategoriAdi, COALESCE(SUM(u.MiktarTon), 0) as ToplamMiktar 
                    FROM Tbl_CiftlikUrunleri u 
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID 
                    WHERE u.DurumID = 2
                    GROUP BY k.KategoriAdi";
                DataTable urunData = DatabaseHelper.ExecuteQuery(urunQuery);

                foreach (DataRow row in urunData.Rows)
                {
                    string kategori = row["KategoriAdi"]?.ToString() ?? "Bilinmiyor";
                    double miktar = Convert.ToDouble(row["ToplamMiktar"]);
                    
                    DataPoint dp = new DataPoint();
                    dp.SetValueXY(kategori, miktar);
                    dp.Label = miktar.ToString("N1");
                    barSeries.Points.Add(dp);
                }

                chart2.Series.Add(barSeries);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grafikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Toplam değerleri hesaplar
        /// </summary>
        private void CalculateTotals()
        {
            try
            {
                // Toplam Çiftlik Sayısı
                object ciftlikResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Tbl_Ciftlikler");
                int toplamCiftlik = ciftlikResult != null ? Convert.ToInt32(ciftlikResult) : 0;
                textBox1.Text = toplamCiftlik.ToString();

                // Toplam Ürün Sayısı
                object urunResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Tbl_CiftlikUrunleri");
                int toplamUrun = urunResult != null ? Convert.ToInt32(urunResult) : 0;
                textBox3.Text = toplamUrun.ToString();

                // Toplam Onaylanan Miktar (Ton)
                object miktarResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(MiktarTon), 0) FROM Tbl_CiftlikUrunleri WHERE DurumID = 2");
                double toplamMiktar = miktarResult != null ? Convert.ToDouble(miktarResult) : 0;
                textBox4.Text = toplamMiktar.ToString("N2") + " Ton";

                // TextBox'ları readonly yap
                textBox1.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;

                // Renklendirme
                textBox1.ForeColor = Color.FromArgb(33, 150, 243);
                textBox3.ForeColor = Color.FromArgb(255, 152, 0);
                textBox4.ForeColor = Color.FromArgb(76, 175, 80);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Toplamlar hesaplanırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Excel'e aktarma
        /// </summary>
        private void BtnExcelExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (reportData == null || reportData.Rows.Count == 0)
                {
                    MessageBox.Show("Aktarılacak veri bulunamadı!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                saveDialog.Title = "Genel Raporu Kaydet";
                saveDialog.FileName = $"Ziraat_GenelRapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelHelper.ExportToExcel(reportData, saveDialog.FileName, "ZİRAAT ODASI GENEL RAPORU",
                        "Toplam Kayıtlı Çiftlik", textBox1.Text,
                        "Toplam Ürün", textBox3.Text,
                        "Onaylanan Toplam", textBox4.Text);

                    DatabaseHelper.LogIslem($"Ziraat Genel Raporu Excel'e aktarıldı - {Session.KullaniciAdi}");

                    MessageBox.Show($"Rapor başarıyla kaydedildi!\n\nKonum: {saveDialog.FileName}", 
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel aktarımı sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Çİftçi_Dasboard dashboard = new Çİftçi_Dasboard();
                dashboard.Show();
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

        private void button1_Click(object sender, EventArgs e)
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

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                SdkRapor sdkRapor = new SdkRapor();
                sdkRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCikis_Click(object sender, EventArgs e)
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

        private void BtnYardim_Click(object sender, EventArgs e)
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
    }
}
