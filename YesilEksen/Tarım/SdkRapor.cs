using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OfficeOpenXml;

namespace YesilEksen.Tarım
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
                label1.Text = "Ziraat Odası - SDG Raporu";

                // GroupBox başlıklarını güncelle
                groupBox1.Text = "Sürdürülebilirlik Değerleri";
                groupBox3.Text = "Grafiksel Analiz";

                // Event handler'ları bağla
                button2.Click += BtnExcelExport_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                btnYardım.Click += BtnYardim_Click;

                // Verileri yükle
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
        /// Grafikleri yükler
        /// </summary>
        private void LoadCharts()
        {
            try
            {
                // ========== Chart1 - Pasta Grafik (Çevresel Etki) ==========
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
        /// Toplam değerleri hesaplar ve label'lara yazar
        /// </summary>
        private void CalculateTotals()
        {
            try
            {
                // Toplam Geri Kazanılan Atık
                object atikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(GeriKazanilanAtikTon), 0) FROM Tbl_SdgRaporVerisi");
                double toplamAtik = atikResult != null ? Convert.ToDouble(atikResult) : 0;

                // Toplam Engellenen CO2
                object co2Result = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(EngellenenCO2Ton), 0) FROM Tbl_SdgRaporVerisi");
                double toplamCO2 = co2Result != null ? Convert.ToDouble(co2Result) : 0;

                // Toplam Ekonomik Değer
                object ekonomikResult = DatabaseHelper.ExecuteScalar(
                    "SELECT COALESCE(SUM(EkonomikDegerTL), 0) FROM Tbl_SdgRaporVerisi");
                double toplamEkonomik = ekonomikResult != null ? Convert.ToDouble(ekonomikResult) : 0;

                // Label'ları güncelle
                label2.Text = $"🌿 Geri Kazanılan Atık: {toplamAtik:N2} Ton";
                label3.Text = $"💨 Engellenen CO₂: {toplamCO2:N2} Ton";
                label4.Text = $"₺ Ekonomiye Kazandırılan: {toplamEkonomik:N2} TL";

                // Eşdeğer hesaplamalar
                int agacEsdeger = (int)(toplamCO2 * 50); // 1 ton CO2 = yaklaşık 50 ağaç
                int aracEsdeger = (int)(toplamCO2 / 4.6); // 1 araç yılda ~4.6 ton CO2

                label5.Text = $"🚗 [{aracEsdeger:N0}] aracın trafikten çekilmesine eşdeğer.";
                label6.Text = $"🌳 [{agacEsdeger:N0}] ağacın bir yıllık emeğine eşdeğer.";

                // Renklendirme
                label2.ForeColor = Color.FromArgb(76, 175, 80);
                label3.ForeColor = Color.FromArgb(33, 150, 243);
                label4.ForeColor = Color.FromArgb(255, 152, 0);
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
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";
                saveDialog.Title = "SDG Raporunu Kaydet";
                saveDialog.FileName = $"Ziraat_SDG_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // SDG Raporu için özel export (veri tablosu yok, sadece özet bilgiler var)
                    ExportSdkRaporToExcel(saveDialog.FileName, "ZİRAAT ODASI - SDG RAPORU");

                    DatabaseHelper.LogIslem($"Ziraat SDG Raporu Excel'e aktarıldı - {Session.KullaniciAdi}");

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

        /// <summary>
        /// SDG Raporunu Excel formatına aktarır
        /// </summary>
        private void ExportSdkRaporToExcel(string filePath, string reportTitle)
        {
            // EPPlus lisans ayarı zaten ExcelHelper static constructor'da yapılıyor
            // Burada tekrar ayarlamaya gerek yok

            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Rapor");
                int row = 1;

                // Başlık
                worksheet.Cells[row, 1].Value = $"YEŞİL EKSEN - {reportTitle}";
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;

                worksheet.Cells[row, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";
                row++;

                worksheet.Cells[row, 1].Value = $"Oluşturan: {Session.KullaniciAdi}";
                row++;

                // Boş satır
                row++;

                // Sürdürülebilirlik Değerleri
                worksheet.Cells[row, 1].Value = "=== SÜRDÜRÜLEBİLİRLİK DEĞERLERİ ===";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 12;
                row++;

                worksheet.Cells[row, 1].Value = label2.Text;
                row++;

                worksheet.Cells[row, 1].Value = label3.Text;
                row++;

                worksheet.Cells[row, 1].Value = label4.Text;
                row++;

                // Boş satır
                row++;

                // Eşdeğer Etkiler
                worksheet.Cells[row, 1].Value = "=== EŞDEĞER ETKİLER ===";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 12;
                row++;

                worksheet.Cells[row, 1].Value = label5.Text;
                row++;

                worksheet.Cells[row, 1].Value = label6.Text;
                row++;

                // Sütun genişliklerini otomatik ayarla
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Dosyayı kaydet
                FileInfo fileInfo = new FileInfo(filePath);
                excel.SaveAs(fileInfo);
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
                GenelRapor genelRapor = new GenelRapor();
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

        private void button1_Click(object sender, EventArgs e)
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

        private void button9_Click(object sender, EventArgs e)
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

        private void label3_Click(object sender, EventArgs e)
        {
            // Label click event
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ComboBox selection changed
        }
    }
}
