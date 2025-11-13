using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace YesilEksen
{
    public partial class FirmaDetay : Form
    {
        public FirmaDetay()
        {
            InitializeComponent();
        }

        private void FirmaDetay_Load(object sender, EventArgs e)
        {
            // Grafiği temizle
            charthacimgrafik.Series.Clear();
            charthacimgrafik.ChartAreas.Clear();

            // 1. Bir "ChartArea" (Grafik Alanı) ekle ve siyah yap
            ChartArea chartArea = new ChartArea("AnaAlan");
            chartArea.BackColor = Color.Black;

            // Eksen çizgilerini ve yazılarını ayarla
            chartArea.AxisX.MajorGrid.LineColor = Color.DimGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.DimGray;
            chartArea.AxisX.LineColor = Color.White;
            chartArea.AxisY.LineColor = Color.White;
            chartArea.AxisX.LabelStyle.ForeColor = Color.White;
            chartArea.AxisY.LabelStyle.ForeColor = Color.White;
            chartArea.AxisY.Title = "kg"; // Y ekseni başlığı
            chartArea.AxisY.TitleForeColor = Color.White;

            charthacimgrafik.ChartAreas.Add(chartArea);

            // 2. Bir "Series" (Veri Serisi) ekle
            Series series = new Series("Aylık Alım Hacmi");
            series.ChartType = SeriesChartType.Column; // Sütun Grafik
            series.Color = Color.Gray; // Sütunların rengi

            // 3. Test verilerini ekle
            series.Points.AddXY("Mayıs", 400);
            series.Points.AddXY("Haziran", 750);
            series.Points.AddXY("Temmuz", 600);
            series.Points.AddXY("Ağustos", 900);
            series.Points.AddXY("Eylül", 500);
            series.Points.AddXY("Ekim", 700);

            // 4. Seriyi grafiğe ekle
            charthacimgrafik.Series.Add(series);

            // 5. Grafiğin başlığını (Legend) gizle
            charthacimgrafik.Legends.Clear();
        }
    }
}
