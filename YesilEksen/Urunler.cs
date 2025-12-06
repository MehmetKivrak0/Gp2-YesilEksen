using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace YesilEksen
{
    public partial class Urunler : Form
    {
        private int currentPage = 1;
        private int itemsPerPage = 12; // Sayfa başına kart sayısı
        private int totalItems = 0;
        private int totalPages = 0;
        private string currentFilterUrun = "";
        private string currentFilterKategori = "";

        public Urunler()
        {
            InitializeComponent();
        }

        private void Urunler_Load(object sender, EventArgs e)
        {
            try
            {
                // Form boyutunu ayarla
                this.Size = new Size(1280, 750);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Kategorileri yükle
                LoadKategoriler();

                // Ürünleri yükle
                LoadUrunler();

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
        /// Kategorileri ComboBox'a yükler
        /// </summary>
        private void LoadKategoriler()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Tüm Kategoriler");

                string query = "SELECT KategoriAdi FROM Tbl_UrunKategorileri ORDER BY KategoriAdi";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    comboBox1.Items.Add(row["KategoriAdi"].ToString());
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategoriler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ürünleri veritabanından yükler
        /// </summary>
        private void LoadUrunler(string urunAdi = "", string kategori = "", bool resetPage = true)
        {
            try
            {
                if (resetPage)
                {
                    currentPage = 1;
                    currentFilterUrun = urunAdi;
                    currentFilterKategori = kategori;
                }

                flpkartlar.Controls.Clear();

                // Toplam kayıt sayısını al
                string countQuery = @"
                    SELECT COUNT(*) as Total
                    FROM Tbl_CiftlikUrunleri u
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID
                    LEFT JOIN Tbl_Ciftlikler c ON u.CiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON u.DurumID = d.DurumID
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(currentFilterUrun))
                {
                    countQuery += $" AND u.UrunAdi LIKE '%{currentFilterUrun}%'";
                }

                if (!string.IsNullOrWhiteSpace(currentFilterKategori) && currentFilterKategori != "Tüm Kategoriler")
                {
                    countQuery += $" AND k.KategoriAdi = '{currentFilterKategori}'";
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
                        u.UrunID, u.UrunAdi, u.MiktarTon,
                        k.KategoriAdi, c.Unvan as CiftlikUnvan,
                        sh.SehirAdi, d.DurumAdi
                    FROM Tbl_CiftlikUrunleri u
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID
                    LEFT JOIN Tbl_Ciftlikler c ON u.CiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON u.DurumID = d.DurumID
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(currentFilterUrun))
                {
                    query += $" AND u.UrunAdi LIKE '%{currentFilterUrun}%'";
                }

                if (!string.IsNullOrWhiteSpace(currentFilterKategori) && currentFilterKategori != "Tüm Kategoriler")
                {
                    query += $" AND k.KategoriAdi = '{currentFilterKategori}'";
                }

                query += $" ORDER BY u.UrunAdi LIMIT {itemsPerPage} OFFSET {offset}";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    Kartlar yeniKart = new Kartlar();
                    yeniKart.Baslik = row["UrunAdi"]?.ToString() ?? "";
                    
                    // Kategori ve Miktar bilgisini Sektor satırına ekle
                    string kategoriAdi = row["KategoriAdi"]?.ToString() ?? "";
                    string miktar = row["MiktarTon"]?.ToString() ?? "0";
                    yeniKart.Sektor = $"{kategoriAdi} | {miktar} Ton";
                    
                    // Şehir ve Çiftlik bilgisini Sehir satırına ekle
                    string sehirAdi = row["SehirAdi"]?.ToString() ?? "";
                    string ciftlik = row["CiftlikUnvan"]?.ToString() ?? "";
                    
                    // Şehir ve çiftlik bilgisini birleştir (kısaltma yok, tam metin)
                    if (!string.IsNullOrEmpty(sehirAdi) && !string.IsNullOrEmpty(ciftlik))
                    {
                        yeniKart.Sehir = $"{sehirAdi} | {ciftlik}";
                    }
                    else if (!string.IsNullOrEmpty(sehirAdi))
                    {
                        yeniKart.Sehir = sehirAdi;
                    }
                    else if (!string.IsNullOrEmpty(ciftlik))
                    {
                        yeniKart.Sehir = $"Çiftlik: {ciftlik}";
                    }
                    else
                    {
                        yeniKart.Sehir = "";
                    }
                    
                    yeniKart.Durum = row["DurumAdi"]?.ToString() ?? "";
                    
                    // Detay butonu gizle
                    yeniKart.DetayButonuGoster = false;
                    yeniKart.Cursor = Cursors.Default;

                    flpkartlar.Controls.Add(yeniKart);
                }

                // Sayfalama bilgisini güncelle
                UpdatePaginationLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sayfalama label'larını günceller
        /// </summary>
        private void UpdatePaginationLabels()
        {
            lblsayfano.Text = $"Sayfa {currentPage} / {totalPages} (Toplam: {totalItems} Ürün)";
            
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
                LoadUrunler(currentFilterUrun, currentFilterKategori, false);
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
                LoadUrunler(currentFilterUrun, currentFilterKategori, false);
            }
        }

        private void BtnFiltrele_Click(object sender, EventArgs e)
        {
            string urunAdi = textBox1.Text.Trim();
            string kategori = comboBox1.SelectedIndex > 0 ? comboBox1.SelectedItem.ToString() : "";
            LoadUrunler(urunAdi, kategori, true);
        }

        private void BtnTemizle_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            LoadUrunler("", "", true);
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            Tarım.Çİftçi_Dasboard dashboard = new Tarım.Çİftçi_Dasboard();
            dashboard.Show();
            this.Close();
        }

        private void BtnYardim_Click(object sender, EventArgs e)
        {
            Yardim yardim = new Yardim();
            yardim.ShowDialog();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // ToolStrip item clicked event
        }
    }
}

