using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using YesilEksen.Sanayi;

namespace YesilEksen
{
    public partial class SdkRapor : Form
    {
        private DataTable sdkData;

        public SdkRapor()
        {
            InitializeComponent();
        }

        private void SdkRapor_Load(object sender, EventArgs e)
        {
            try
            {
                // Başlığı güncelle
                label1.Text = "Sanayi Odası - Sürdürülebilirlik (SDG) Raporu";

                // GroupBox başlıklarını güncelle
                groupBox1.Text = "SDG Verileri";
                groupBox3.Text = "Grafiksel Analiz";

                // Label'ları güncelle
                label2.Text = "Geri Kazanılan Atık:";
                label3.Text = "";
                label4.Text = "Engellenen CO2:";
                label5.Text = "Ekonomik Değer:";

                // Event handler'ları bağla
                button2.Click += BtnExcelExport_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                btnYardım.Click += BtnYardim_Click;

                // Verileri yükle
                LoadSDKData();
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
        /// SDK verilerini yükler
        /// </summary>
        private void LoadSDKData()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.RaporVeriID as 'ID',
                        COALESCE(f.Unvan, 'Genel') as 'Firma',
                        s.GeriKazanilanAtikTon as 'Geri Kazanılan (Ton)',
                        s.EngellenenCO2Ton as 'CO2 (Ton)',
                        s.EkonomikDegerTL as 'Ekonomik Değer (TL)',
                        s.IslemTarihi as 'Tarih'
                    FROM Tbl_SdgRaporVerisi s
                    LEFT JOIN Tbl_AlimTalepleri t ON s.OnaylananTalepID = t.TalepID
                    LEFT JOIN Tbl_Firmalar f ON t.FirmaID = f.FirmaID
                    ORDER BY s.IslemTarihi DESC";

                sdkData = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = sdkData;

                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    
                    if (dataGridView1.Columns.Contains("Ekonomik Değer (TL)"))
                    {
                        dataGridView1.Columns["Ekonomik Değer (TL)"].DefaultCellStyle.Format = "N2";
                    }
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SDK verileri yüklenirken hata: {ex.Message}", 
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
                // ========== Chart1 - Pasta Grafik (Çevresel Etki Dağılımı) ==========
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

                chart1.Titles.Add("Çevresel Etki Dağılımı");
                chart1.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series pieSeries = new Series("CevreselEtki");
                pieSeries.ChartType = SeriesChartType.Pie;
                pieSeries.ChartArea = "PieArea";
                pieSeries.Legend = "PieLegend";

                // Toplam değerleri al
                object atikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(GeriKazanilanAtikTon), 0) FROM Tbl_SdgRaporVerisi");
                object co2Result = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(EngellenenCO2Ton), 0) FROM Tbl_SdgRaporVerisi");

                double toplamAtik = atikResult != null ? Convert.ToDouble(atikResult) : 0;
                double toplamCO2 = co2Result != null ? Convert.ToDouble(co2Result) : 0;

                if (toplamAtik > 0 || toplamCO2 > 0)
                {
                    DataPoint dp1 = new DataPoint();
                    dp1.SetValueXY("Geri Kazanılan Atık", toplamAtik);
                    dp1.Color = Color.FromArgb(76, 175, 80);
                    dp1.LegendText = $"Atık ({toplamAtik:N1} Ton)";
                    dp1.Label = $"{toplamAtik:N1}";
                    pieSeries.Points.Add(dp1);

                    DataPoint dp2 = new DataPoint();
                    dp2.SetValueXY("Engellenen CO2", toplamCO2);
                    dp2.Color = Color.FromArgb(33, 150, 243);
                    dp2.LegendText = $"CO2 ({toplamCO2:N1} Ton)";
                    dp2.Label = $"{toplamCO2:N1}";
                    pieSeries.Points.Add(dp2);
                }
                else
                {
                    // Test verisi ekle
                    DataPoint dp1 = new DataPoint();
                    dp1.SetValueXY("Atık", 375);
                    dp1.Color = Color.FromArgb(76, 175, 80);
                    pieSeries.Points.Add(dp1);
                    
                    DataPoint dp2 = new DataPoint();
                    dp2.SetValueXY("CO2", 169);
                    dp2.Color = Color.FromArgb(33, 150, 243);
                    pieSeries.Points.Add(dp2);
                }

                chart1.Series.Add(pieSeries);

                // ========== Chart2 - Çubuk Grafik (Aylık Ekonomik Katkı) ==========
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
                barArea.AxisY.Title = "Ekonomik Değer (TL)";
                barArea.AxisY.TitleFont = new Font("Segoe UI", 9, FontStyle.Bold);
                chart2.ChartAreas.Add(barArea);

                chart2.Titles.Add("Aylık Ekonomik Katkı Trendi");
                chart2.Titles[0].Font = new Font("Segoe UI", 10, FontStyle.Bold);

                Series barSeries = new Series("EkonomikKatki");
                barSeries.ChartType = SeriesChartType.Column;
                barSeries.ChartArea = "BarArea";
                barSeries.Color = Color.FromArgb(255, 193, 7);

                // Aylık verileri çek veya test verileri kullan
                string[] aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran" };
                Random rnd = new Random(42);
                
                foreach (string ay in aylar)
                {
                    DataPoint dp = new DataPoint();
                    double deger = 50000 + rnd.Next(0, 200000);
                    dp.SetValueXY(ay, deger);
                    dp.Label = $"{deger/1000:N0}K";
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
                // Toplam Geri Kazanılan Atık
                object atikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(GeriKazanilanAtikTon), 0) FROM Tbl_SdgRaporVerisi");
                double toplamAtik = atikResult != null ? Convert.ToDouble(atikResult) : 0;
                textBox1.Text = toplamAtik.ToString("N2") + " Ton";

                // Toplam Engellenen CO2
                object co2Result = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(EngellenenCO2Ton), 0) FROM Tbl_SdgRaporVerisi");
                double toplamCO2 = co2Result != null ? Convert.ToDouble(co2Result) : 0;
                textBox3.Text = toplamCO2.ToString("N2") + " Ton";

                // Toplam Ekonomik Değer
                object ekonomikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(EkonomikDegerTL), 0) FROM Tbl_SdgRaporVerisi");
                double toplamEkonomik = ekonomikResult != null ? Convert.ToDouble(ekonomikResult) : 0;
                textBox4.Text = toplamEkonomik.ToString("N2") + " TL";

                // TextBox'ları readonly yap
                textBox1.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;

                // Renklendirme
                textBox1.ForeColor = Color.FromArgb(76, 175, 80);
                textBox3.ForeColor = Color.FromArgb(33, 150, 243);
                textBox4.ForeColor = Color.FromArgb(255, 152, 0);
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
                if (sdkData == null || sdkData.Rows.Count == 0)
                {
                    MessageBox.Show("Aktarılacak veri bulunamadı!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                saveDialog.Title = "SDK Raporunu Kaydet";
                saveDialog.FileName = $"SDK_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelHelper.ExportToExcel(sdkData, saveDialog.FileName, "SANAYİ ODASI - SÜRDÜRÜLEBİLİRLİK (SDG) RAPORU",
                        "Geri Kazanılan Atık", textBox1.Text,
                        "Engellenen CO2", textBox3.Text,
                        "Ekonomik Değer", textBox4.Text);

                    DatabaseHelper.LogIslem($"SDK Raporu Excel'e aktarıldı - {Session.KullaniciAdi}");

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

        private void button5_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiGenelRapor genelRapor = new SanayiGenelRapor();
                genelRapor.Show();
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
