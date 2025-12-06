using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace YesilEksen
{
    public partial class GenelRapor : Form
    {
        private DataTable raporData;

        public GenelRapor()
        {
            InitializeComponent();
        }

        private void button7_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
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

        private void GenelRapor_Load(object sender, EventArgs e)
        {
            try
            {
                // Başlığı güncelle
                label1.Text = "Sanayi Odası - Genel Rapor";

                // Event handler'ları bağla
                button1.Click += BtnExcelExport_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                btnYardım.Click += BtnYardim_Click;

                // Verileri yükle
                LoadReportData();
                LoadCharts();
                CalculateSummary();
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
                        f.Unvan as 'Firma',
                        c.Unvan as 'Çiftlik',
                        u.UrunAdi as 'Ürün',
                        t.TalepMiktarTon as 'Miktar (Ton)',
                        d.DurumAdi as 'Durum',
                        t.TalepTarihi as 'Tarih'
                    FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_Firmalar f ON t.FirmaID = f.FirmaID
                    LEFT JOIN Tbl_Ciftlikler c ON t.HedefCiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_CiftlikUrunleri u ON t.UrunID = u.UrunID
                    LEFT JOIN Tbl_OnayDurumlari d ON t.DurumID = d.DurumID
                    ORDER BY t.TalepTarihi DESC";

                raporData = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = raporData;

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
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
                // Chart1 - Pasta Grafik (Durum Dağılımı)
                chart1.Series.Clear();
                chart1.ChartAreas.Clear();
                chart1.Legends.Clear();

                ChartArea pieArea = new ChartArea("PieArea");
                chart1.ChartAreas.Add(pieArea);

                Legend pieLegend = new Legend("PieLegend");
                chart1.Legends.Add(pieLegend);

                Series pieSeries = new Series("DurumDagilimi");
                pieSeries.ChartType = SeriesChartType.Pie;
                pieSeries.ChartArea = "PieArea";
                pieSeries.Legend = "PieLegend";

                // Duruma göre sayıları al
                DataTable durumData = DatabaseHelper.ExecuteQuery(@"
                    SELECT d.DurumAdi, COUNT(*) as Sayi 
                    FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_OnayDurumlari d ON t.DurumID = d.DurumID
                    GROUP BY t.DurumID");

                foreach (DataRow row in durumData.Rows)
                {
                    DataPoint dp = new DataPoint();
                    dp.SetValueXY(row["DurumAdi"].ToString(), Convert.ToInt32(row["Sayi"]));
                    dp.Label = $"{row["DurumAdi"]} ({row["Sayi"]})";
                    pieSeries.Points.Add(dp);
                }

                chart1.Series.Add(pieSeries);
                chart1.Titles.Clear();
                chart1.Titles.Add("Talep Durum Dağılımı");

                // Chart2 - Çubuk Grafik (Aylık Talep Miktarı)
                chart2.Series.Clear();
                chart2.ChartAreas.Clear();
                chart2.Legends.Clear();

                ChartArea barArea = new ChartArea("BarArea");
                barArea.AxisX.Title = "Sektör";
                barArea.AxisY.Title = "Talep (Ton)";
                chart2.ChartAreas.Add(barArea);

                Series barSeries = new Series("SektorTalep");
                barSeries.ChartType = SeriesChartType.Column;
                barSeries.ChartArea = "BarArea";
                barSeries.Color = Color.ForestGreen;

                // Sektöre göre toplam talep
                DataTable sektorData = DatabaseHelper.ExecuteQuery(@"
                    SELECT s.SektorAdi, COALESCE(SUM(t.TalepMiktarTon), 0) as ToplamTalep
                    FROM Tbl_Sektorler s
                    LEFT JOIN Tbl_Firmalar f ON s.SektorID = f.SektorID
                    LEFT JOIN Tbl_AlimTalepleri t ON f.FirmaID = t.FirmaID
                    GROUP BY s.SektorID
                    ORDER BY ToplamTalep DESC
                    LIMIT 5");

                foreach (DataRow row in sektorData.Rows)
                {
                    barSeries.Points.AddXY(row["SektorAdi"].ToString(), 
                        Convert.ToDouble(row["ToplamTalep"]));
                }

                chart2.Series.Add(barSeries);
                chart2.Titles.Clear();
                chart2.Titles.Add("Sektörlere Göre Talep");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grafikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Özet hesaplamalarını yapar
        /// </summary>
        private void CalculateSummary()
        {
            try
            {
                // Toplam Arz (Çiftliklerden)
                object arzResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(MiktarTon), 0) FROM Tbl_CiftlikUrunleri WHERE DurumID = 2");
                double toplamArz = arzResult != null ? Convert.ToDouble(arzResult) : 0;
                textBox1.Text = toplamArz.ToString("N2");

                // Toplam Talep
                object talepResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(TalepMiktarTon), 0) FROM Tbl_AlimTalepleri");
                double toplamTalep = talepResult != null ? Convert.ToDouble(talepResult) : 0;
                textBox3.Text = toplamTalep.ToString("N2") + " Ton";

                // Net (Arz - Talep)
                double net = toplamArz - toplamTalep;
                textBox4.Text = net.ToString("N2") + " Ton";
                textBox4.ForeColor = net >= 0 ? Color.Green : Color.Red;

                // TextBox'ları readonly yap
                textBox1.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Özet hesaplanırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Excel'e aktarma işlemi
        /// </summary>
        private void BtnExcelExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (raporData == null || raporData.Rows.Count == 0)
                {
                    MessageBox.Show("Aktarılacak veri bulunamadı!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                saveDialog.Title = "Raporu Kaydet";
                saveDialog.FileName = $"GenelRapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelHelper.ExportToExcel(raporData, saveDialog.FileName, "GENEL RAPOR",
                        "Toplam Arz", textBox1.Text,
                        "Toplam Talep", textBox3.Text,
                        "Net", textBox4.Text);

                    // Log kaydet
                    DatabaseHelper.LogIslem($"Genel Rapor Excel'e aktarıldı - {Session.KullaniciAdi}");

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

        private void btnsdkrapor_Click(object sender, EventArgs e)
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

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
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
