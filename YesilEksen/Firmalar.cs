using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Firmalar : Form
    {
        private int currentPage = 1;
        private int itemsPerPage = 12; // Sayfa başına kart sayısı
        private int totalItems = 0;
        private int totalPages = 0;
        private string currentFilterFirma = "";
        private string currentFilterSehir = "";

        public Firmalar()
        {
            InitializeComponent();
        }

        private void Firmalar_Load(object sender, EventArgs e)
        {
            try
            {
                // Form boyutunu ayarla
                this.Size = new Size(1280, 750);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Şehirleri yükle
                LoadSehirler();

                // Firmaları yükle
                LoadFirmalar();

                // Event handler'ları bağla
                button1.Click += BtnFiltrele_Click;
                button2.Click += BtnTemizle_Click;
                geriToolStripMenuItem.Click += BtnGeri_Click;
                yardımToolStripMenuItem.Click += BtnYardim_Click;
                
                // Sayfalama event handler'ları
                lblonceki.Click += LblOnceki_Click;
                lblsonraki.Click += LblSonraki_Click;
                lblonceki.Cursor = Cursors.Hand;
                lblsonraki.Cursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Şehirleri ComboBox'a yükler
        /// </summary>
        private void LoadSehirler()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Tüm Şehirler");

                string query = "SELECT SehirAdi FROM Tbl_Sehirler ORDER BY SehirAdi";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    comboBox1.Items.Add(row["SehirAdi"].ToString());
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şehirler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Firmaları veritabanından yükler
        /// </summary>
        private void LoadFirmalar(string firmaAdi = "", string sehir = "", bool resetPage = true)
        {
            try
            {
                if (resetPage)
                {
                    currentPage = 1;
                    currentFilterFirma = firmaAdi;
                    currentFilterSehir = sehir;
                }

                flpkartlar.Controls.Clear();

                // Toplam kayıt sayısını al
                string countQuery = @"
                    SELECT COUNT(*) as Total
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(currentFilterFirma))
                {
                    countQuery += $" AND f.Unvan LIKE '%{currentFilterFirma}%'";
                }

                if (!string.IsNullOrWhiteSpace(currentFilterSehir) && currentFilterSehir != "Tüm Şehirler")
                {
                    countQuery += $" AND sh.SehirAdi = '{currentFilterSehir}'";
                }

                totalItems = Convert.ToInt32(DatabaseHelper.ExecuteScalar(countQuery));
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

                if (totalPages == 0) totalPages = 1;
                if (currentPage > totalPages) currentPage = totalPages;
                if (currentPage < 1) currentPage = 1;

                // Sayfalama ile veri çek
                int offset = (currentPage - 1) * itemsPerPage;
                string query = @"
                    SELECT 
                        f.FirmaID, f.Unvan, f.VergiNo, f.Adres,
                        s.SektorAdi, sh.SehirAdi, d.DurumAdi
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(currentFilterFirma))
                {
                    query += $" AND f.Unvan LIKE '%{currentFilterFirma}%'";
                }

                if (!string.IsNullOrWhiteSpace(currentFilterSehir) && currentFilterSehir != "Tüm Şehirler")
                {
                    query += $" AND sh.SehirAdi = '{currentFilterSehir}'";
                }

                query += $" ORDER BY f.Unvan LIMIT {itemsPerPage} OFFSET {offset}";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    Kartlar yeniKart = new Kartlar();
                    yeniKart.Baslik = row["Unvan"]?.ToString() ?? "";
                    yeniKart.Sektor = row["SektorAdi"]?.ToString() ?? "";
                    yeniKart.Sehir = row["SehirAdi"]?.ToString() ?? "";
                    yeniKart.Durum = row["DurumAdi"]?.ToString() ?? "";
                    
                    // Kart tıklama eventi
                    int firmaID = Convert.ToInt32(row["FirmaID"]);
                    yeniKart.Tag = firmaID;
                    
                    // Detay butonuna tıklandığında
                    yeniKart.DetayClick += (s, ev) => OpenFirmaDetay(firmaID);
                    yeniKart.Cursor = Cursors.Hand;

                    flpkartlar.Controls.Add(yeniKart);
                }

                // Sayfalama bilgisini güncelle
                UpdatePaginationLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firmalar yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sayfalama label'larını günceller
        /// </summary>
        private void UpdatePaginationLabels()
        {
            lblsayfano.Text = $"Sayfa {currentPage} / {totalPages} (Toplam: {totalItems} Firma)";
            
            // Önceki/Sonraki butonlarını aktif/pasif yap
            lblonceki.Enabled = currentPage > 1;
            lblsonraki.Enabled = currentPage < totalPages;
            
            if (currentPage > 1)
            {
                lblonceki.ForeColor = Color.Blue;
            }
            else
            {
                lblonceki.ForeColor = Color.Gray;
            }
            
            if (currentPage < totalPages)
            {
                lblsonraki.ForeColor = Color.Blue;
            }
            else
            {
                lblsonraki.ForeColor = Color.Gray;
            }
        }

        /// <summary>
        /// Önceki sayfa
        /// </summary>
        private void LblOnceki_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadFirmalar(currentFilterFirma, currentFilterSehir, false);
            }
        }

        /// <summary>
        /// Sonraki sayfa
        /// </summary>
        private void LblSonraki_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadFirmalar(currentFilterFirma, currentFilterSehir, false);
            }
        }

        /// <summary>
        /// Firma detay sayfasını açar
        /// </summary>
        private void OpenFirmaDetay(int firmaID)
        {
            try
            {
                FirmaDetay detay = new FirmaDetay();
                detay.FirmaID = firmaID;
                detay.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnFiltrele_Click(object sender, EventArgs e)
        {
            string firmaAdi = textBox1.Text.Trim();
            string sehir = comboBox1.SelectedIndex > 0 ? comboBox1.SelectedItem.ToString() : "";
            LoadFirmalar(firmaAdi, sehir, true);
        }

        private void BtnTemizle_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            LoadFirmalar("", "", true);
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Close();
        }

        private void BtnYardim_Click(object sender, EventArgs e)
        {
            Yardim yardim = new Yardim();
            yardim.ShowDialog();
        }

        private void kartlar1_Load(object sender, EventArgs e)
        {
        }
    }
}
