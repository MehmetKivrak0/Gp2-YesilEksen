using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace YesilEksen.Sanayi
{
    public partial class SanayiGenelRapor : Form
    {
        private DataTable reportData;

        public SanayiGenelRapor()
        {
            InitializeComponent();
        }

        private void SanayiGenelRapor_Load(object sender, EventArgs e)
        {
            try
            {
                // Başlığı güncelle
                label1.Text = "Sanayi Odası - Genel Rapor";

                // GroupBox başlıklarını güncelle
                groupBox1.Text = "Firma ve Talep Verileri";
                groupBox2.Text = "Özet İstatistikler";
                groupBox3.Text = "Grafiksel Analiz";

                // Label'ları güncelle
                label2.Text = "Toplam Firma:";
                label3.Text = "";
                label4.Text = "Toplam Talep:";
                label5.Text = "Onaylı Miktar:";

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
                        f.FirmaID as 'ID',
                        f.Unvan as 'Firma Ünvanı',
                        s.SektorAdi as 'Sektör',
                        sh.SehirAdi as 'Şehir',
                        d.DurumAdi as 'Durum',
                        (SELECT COUNT(*) FROM Tbl_AlimTalepleri WHERE FirmaID = f.FirmaID) as 'Talep Sayısı',
                        (SELECT COALESCE(SUM(TalepMiktarTon), 0) FROM Tbl_AlimTalepleri WHERE FirmaID = f.FirmaID AND DurumID = 2) as 'Onaylı (Ton)'
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    ORDER BY f.Unvan";

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
                // ========== Chart1 - Pasta Grafik (Sektörlere Göre Firma Dağılımı) ==========
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

                chart1.Titles.Add("Sektörlere Göre Firma Dağılımı");
                chart1.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series pieSeries = new Series("SektorDagilimi");
                pieSeries.ChartType = SeriesChartType.Pie;
                pieSeries.ChartArea = "PieArea";
                pieSeries.Legend = "PieLegend";

                // Sektöre göre firma sayılarını al
                string sektorQuery = @"
                    SELECT s.SektorAdi, COUNT(*) as Sayi 
                    FROM Tbl_Firmalar f 
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID 
                    GROUP BY s.SektorAdi";
                DataTable sektorData = DatabaseHelper.ExecuteQuery(sektorQuery);

                Color[] colors = { Color.FromArgb(76, 175, 80), Color.FromArgb(33, 150, 243), 
                                   Color.FromArgb(255, 152, 0), Color.FromArgb(156, 39, 176), 
                                   Color.FromArgb(244, 67, 54) };
                int colorIndex = 0;

                foreach (DataRow row in sektorData.Rows)
                {
                    DataPoint dp = new DataPoint();
                    string sektor = row["SektorAdi"]?.ToString() ?? "Bilinmiyor";
                    int sayi = Convert.ToInt32(row["Sayi"]);
                    dp.SetValueXY(sektor, sayi);
                    dp.Color = colors[colorIndex % colors.Length];
                    dp.LegendText = $"{sektor} ({sayi})";
                    dp.Label = sayi.ToString();
                    pieSeries.Points.Add(dp);
                    colorIndex++;
                }

                chart1.Series.Add(pieSeries);

                // ========== Chart2 - Çubuk Grafik (Talep Durumları) ==========
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
                barArea.AxisY.Title = "Talep Sayısı";
                barArea.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                chart2.ChartAreas.Add(barArea);

                chart2.Titles.Add("Alım Talepleri Durumu");
                chart2.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series barSeries = new Series("TalepDurumlari");
                barSeries.ChartType = SeriesChartType.Column;
                barSeries.ChartArea = "BarArea";

                // Talep durumlarını al
                string talepQuery = @"
                    SELECT d.DurumAdi, COUNT(*) as Sayi 
                    FROM Tbl_AlimTalepleri t 
                    LEFT JOIN Tbl_OnayDurumlari d ON t.DurumID = d.DurumID 
                    GROUP BY d.DurumAdi";
                DataTable talepData = DatabaseHelper.ExecuteQuery(talepQuery);

                foreach (DataRow row in talepData.Rows)
                {
                    string durum = row["DurumAdi"]?.ToString() ?? "Bilinmiyor";
                    int sayi = Convert.ToInt32(row["Sayi"]);
                    
                    DataPoint dp = new DataPoint();
                    dp.SetValueXY(durum, sayi);
                    dp.Label = sayi.ToString();
                    
                    if (durum.Contains("Onaylandı"))
                        dp.Color = Color.FromArgb(76, 175, 80);
                    else if (durum.Contains("Reddedildi"))
                        dp.Color = Color.FromArgb(244, 67, 54);
                    else
                        dp.Color = Color.FromArgb(255, 193, 7);
                    
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
                // Toplam Firma Sayısı
                object firmaResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Tbl_Firmalar");
                int toplamFirma = firmaResult != null ? Convert.ToInt32(firmaResult) : 0;
                textBox1.Text = toplamFirma.ToString();

                // Toplam Talep Sayısı
                object talepResult = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Tbl_AlimTalepleri");
                int toplamTalep = talepResult != null ? Convert.ToInt32(talepResult) : 0;
                textBox3.Text = toplamTalep.ToString();

                // Toplam Onaylanan Miktar (Ton)
                object miktarResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(TalepMiktarTon), 0) FROM Tbl_AlimTalepleri WHERE DurumID = 2");
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
        private void button2_Click(object sender, EventArgs e)
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
                saveDialog.FileName = $"Sanayi_GenelRapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelHelper.ExportToExcel(reportData, saveDialog.FileName, "SANAYİ ODASI GENEL RAPORU",
                        "Toplam Kayıtlı Firma", textBox1.Text,
                        "Toplam Alım Talebi", textBox3.Text,
                        "Onaylanan Toplam", textBox4.Text);

                    DatabaseHelper.LogIslem($"Sanayi Genel Raporu Excel'e aktarıldı - {Session.KullaniciAdi}");

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

        public void button4_Click(object sender, EventArgs e)
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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiFirmaOnay sanayiFirmaOnay = new SanayiFirmaOnay();
                sanayiFirmaOnay.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Form1 form1 = new Form1();
                form1.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnÇıkışYap_Click(object sender, EventArgs e)
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

        private void btnYardım_Click(object sender, EventArgs e)
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
