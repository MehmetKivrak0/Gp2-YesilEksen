using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YesilEksen.Tarım;

namespace YesilEksen.Tarım
{
    public partial class ÇitflikÜrünOnay : Form
    {
        private int selectedUrunID = 0;

        public ÇitflikÜrünOnay()
        {
            InitializeComponent();
        }

        private void ÇitflikÜrünOnay_Load(object sender, EventArgs e)
        {
            try
            {
                // Başlık güncelle
                label1.Text = "Çiftlik Ürün Onay";

                // Event handler'ları bağla
                btnOnayla.Click += BtnOnayla_Click;
                btnReddet.Click += BtnReddet_Click;
                btnGörüntüle.Click += BtnGoruntule_Click;
                button4.Click += BtnYardim_Click;
                button5.Click += BtnCikis_Click;
                dataGridView1.CellClick += DataGridView1_CellClick;
                
                // Filtreleme için Enter tuşu desteği
                textBox1.KeyPress += (s, ex) => {
                    if (ex.KeyChar == (char)Keys.Enter)
                    {
                        button6_Click(null, null);
                        ex.Handled = true;
                    }
                };

                // Filtreleme kontrollerini doldur
                LoadFiltreKontrolleri();

                // Verileri yükle
                LoadUrunler();

                // TextBox'ları readonly yap (reddetme nedeni hariç)
                txtunvan.ReadOnly = true;  // Ürün Adı
                txtvergi.ReadOnly = true;  // Çiftlik
                txtsektör.ReadOnly = true; // Kategori
                txtadres.ReadOnly = true;  // Miktar
                txtbşdetay.ReadOnly = false; // Red Nedeni

                // Label'ları güncelle
                label4.Text = "Ürün Adı:";
                label5.Text = "Çiftlik:";
                label6.Text = "Kategori:";
                label7.Text = "Miktar:";

                groupBox2.Text = "Ürün Bilgileri";
                groupBox1.Text = "Onay Bekleyen Ürünler";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Filtreleme kontrollerini doldurur
        /// </summary>
        private void LoadFiltreKontrolleri()
        {
            try
            {
                // Kategori ComboBox'ını doldur
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Tüm Kategoriler");
                DataTable kategoriler = DatabaseHelper.ExecuteQuery("SELECT DISTINCT KategoriAdi FROM Tbl_UrunKategorileri ORDER BY KategoriAdi");
                foreach (DataRow row in kategoriler.Rows)
                {
                    comboBox1.Items.Add(row["KategoriAdi"].ToString());
                }
                comboBox1.SelectedIndex = 0;

                // Çiftlik ComboBox'ını doldur
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Tüm Çiftlikler");
                DataTable ciftlikler = DatabaseHelper.ExecuteQuery("SELECT DISTINCT Unvan FROM Tbl_Ciftlikler ORDER BY Unvan");
                foreach (DataRow row in ciftlikler.Rows)
                {
                    comboBox2.Items.Add(row["Unvan"].ToString());
                }
                comboBox2.SelectedIndex = 0;

                // Tarih seçiciyi gizle (veritabanında KayitTarihi sütunu yok)
                dateTimePicker1.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtre kontrolleri yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Onay bekleyen ürünleri yükler
        /// </summary>
        private void LoadUrunler(string aramaMetni = "", string kategori = "", string ciftlik = "")
        {
            try
            {
                string query = @"
                    SELECT 
                        u.UrunID,
                        u.UrunAdi as 'Ürün Adı',
                        c.Unvan as 'Çiftlik',
                        k.KategoriAdi as 'Kategori',
                        u.MiktarTon as 'Miktar (Ton)',
                        d.DurumAdi as 'Durum'
                    FROM Tbl_CiftlikUrunleri u
                    LEFT JOIN Tbl_Ciftlikler c ON u.CiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID
                    LEFT JOIN Tbl_OnayDurumlari d ON u.DurumID = d.DurumID
                    WHERE u.DurumID = 1";

                // Filtreleme koşulları
                if (!string.IsNullOrWhiteSpace(aramaMetni))
                {
                    query += $" AND u.UrunAdi LIKE '%{aramaMetni}%'";
                }

                if (!string.IsNullOrWhiteSpace(kategori) && kategori != "Tüm Kategoriler")
                {
                    query += $" AND k.KategoriAdi = '{kategori}'";
                }

                if (!string.IsNullOrWhiteSpace(ciftlik) && ciftlik != "Tüm Çiftlikler")
                {
                    query += $" AND c.Unvan = '{ciftlik}'";
                }

                query += " ORDER BY u.UrunID DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["UrunID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                groupBox1.Text = $"Onay Bekleyen Ürünler ({dt.Rows.Count} adet)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Grid'den satır seçildiğinde
        /// </summary>
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                    selectedUrunID = Convert.ToInt32(row.Cells["UrunID"].Value);
                    LoadUrunDetay(selectedUrunID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seçim hatası: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Seçili ürünün detaylarını yükler
        /// </summary>
        private void LoadUrunDetay(int urunID)
        {
            try
            {
                string query = @"
                    SELECT 
                        u.UrunAdi, u.MiktarTon,
                        c.Unvan as CiftlikUnvan,
                        k.KategoriAdi
                    FROM Tbl_CiftlikUrunleri u
                    LEFT JOIN Tbl_Ciftlikler c ON u.CiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_UrunKategorileri k ON u.UrunKategoriID = k.KategoriID
                    WHERE u.UrunID = @urunID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@urunID", urunID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtunvan.Text = row["UrunAdi"]?.ToString() ?? "";
                    txtvergi.Text = row["CiftlikUnvan"]?.ToString() ?? "";
                    txtsektör.Text = row["KategoriAdi"]?.ToString() ?? "";
                    txtadres.Text = row["MiktarTon"]?.ToString() + " Ton" ?? "";
                    label3.Text = row["UrunAdi"]?.ToString() ?? "[Ürün Adı]";
                }

                // Belgeleri yükle
                LoadBelgeler(urunID);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ürün belgelerini yükler
        /// </summary>
        private void LoadBelgeler(int urunID)
        {
            try
            {
                listBelge.Items.Clear();
                listBelge.Tag = new Dictionary<string, string>(); // BelgeAdi -> DosyaYolu mapping

                string query = "SELECT BelgeAdi, DosyaYolu FROM Tbl_UrunBelgeleri WHERE UrunID = @urunID";
                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@urunID", urunID));

                if (dt.Rows.Count > 0)
                {
                    var belgeMap = (Dictionary<string, string>)listBelge.Tag;
                    foreach (DataRow row in dt.Rows)
                    {
                        string belgeAdi = row["BelgeAdi"]?.ToString() ?? "";
                        string dosyaYolu = row["DosyaYolu"]?.ToString() ?? "";
                        listBelge.Items.Add(belgeAdi);
                        belgeMap[belgeAdi] = dosyaYolu;
                    }
                }
                else
                {
                    listBelge.Items.Add("Belge bulunamadı");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belgeler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ürünü onaylar
        /// </summary>
        private void BtnOnayla_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedUrunID == 0)
                {
                    MessageBox.Show("Lütfen onaylanacak bir ürün seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{txtunvan.Text} ürününü onaylamak istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Ürünü onayla (DurumID = 2)
                    string updateQuery = "UPDATE Tbl_CiftlikUrunleri SET DurumID = 2 WHERE UrunID = @urunID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@urunID", selectedUrunID));

                    if (affected > 0)
                    {
                        // Log kaydet
                        DatabaseHelper.LogIslem($"{txtvergi.Text} çiftliğinin {txtunvan.Text} ürünü {Session.KullaniciAdi} tarafından onaylandı");

                        MessageBox.Show("Ürün başarıyla onaylandı!", 
                            "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadUrunler();
                    }
                    else
                    {
                        MessageBox.Show("Ürün onaylanamadı!", 
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Onaylama sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ürünü reddeder
        /// </summary>
        private void BtnReddet_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedUrunID == 0)
                {
                    MessageBox.Show("Lütfen reddedilecek bir ürün seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtbşdetay.Text))
                {
                    MessageBox.Show("Lütfen red nedenini yazın!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtbşdetay.Focus();
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{txtunvan.Text} ürününü reddetmek istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Ürünü reddet (DurumID = 3)
                    string updateQuery = "UPDATE Tbl_CiftlikUrunleri SET DurumID = 3 WHERE UrunID = @urunID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@urunID", selectedUrunID));

                    if (affected > 0)
                    {
                        // Log kaydet
                        DatabaseHelper.LogIslem($"{txtvergi.Text} çiftliğinin {txtunvan.Text} ürünü {Session.KullaniciAdi} tarafından reddedildi - Neden: {txtbşdetay.Text}");

                        MessageBox.Show("Ürün başvurusu reddedildi!", 
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadUrunler();
                    }
                    else
                    {
                        MessageBox.Show("Ürün reddedilemedi!", 
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reddetme sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Belge görüntüleme
        /// </summary>
        private void BtnGoruntule_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBelge.SelectedItem == null || listBelge.SelectedItem.ToString() == "Belge bulunamadı")
                {
                    MessageBox.Show("Lütfen görüntülenecek bir belge seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string secilenBelgeAdi = listBelge.SelectedItem.ToString();
                var belgeMap = listBelge.Tag as Dictionary<string, string>;

                if (belgeMap == null || !belgeMap.ContainsKey(secilenBelgeAdi))
                {
                    MessageBox.Show("Belge dosya yolu bulunamadı!", 
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string dosyaYolu = belgeMap[secilenBelgeAdi];
                
                // Dosya yolunu tam yola çevir
                string tamYol = dosyaYolu;
                if (!Path.IsPathRooted(dosyaYolu))
                {
                    // Relative path ise, uygulama dizinine göre tam yol oluştur
                    tamYol = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dosyaYolu);
                }

                // Dosyanın varlığını kontrol et
                if (!File.Exists(tamYol))
                {
                    MessageBox.Show($"Belge dosyası bulunamadı!\n\nDosya Yolu: {tamYol}", 
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // PDF dosyasını varsayılan programla aç
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tamYol,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Belge açılırken hata oluştu: {ex.Message}\n\nDosya Yolu: {tamYol}", 
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Belge görüntülenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Formu temizler
        /// </summary>
        private void ClearForm()
        {
            selectedUrunID = 0;
            txtunvan.Clear();
            txtvergi.Clear();
            txtsektör.Clear();
            txtadres.Clear();
            txtbşdetay.Clear();
            listBelge.Items.Clear();
            listBelge.Tag = null;
            label3.Text = "[Seçili Ürünün Adı]";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                YesilEksen.Tarım.SdkRapor sdkRapor = new YesilEksen.Tarım.SdkRapor();
                sdkRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnsdkrapor_Click(object sender, EventArgs e)
        {
            try
            {
                Tarım.GenelRapor genelRapor = new Tarım.GenelRapor();
                genelRapor.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }

        private void pnlAltMenu_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ÇiftlikOnay çiftlikOnay = new ÇiftlikOnay();
                çiftlikOnay.Show();
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

        /// <summary>
        /// Filtreleme butonu
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string aramaMetni = textBox1.Text.Trim();
                string kategori = comboBox1.SelectedItem?.ToString() ?? "";
                string ciftlik = comboBox2.SelectedItem?.ToString() ?? "";

                LoadUrunler(aramaMetni, kategori, ciftlik);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtreleme sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Filtreleri temizle
        /// </summary>
        private void BtnFiltreTemizle_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Clear();
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                LoadUrunler();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtre temizleme sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
