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
    public partial class ÇiftlikOnay : Form
    {
        private int selectedCiftlikID = 0;

        public ÇiftlikOnay()
        {
            InitializeComponent();
        }

        private void ÇiftlikOnay_Load(object sender, EventArgs e)
        {
            try
            {
                // Event handler'ları bağla
                btnOnayla.Click += BtnOnayla_Click;
                btnReddet.Click += BtnReddet_Click;
                btnGörüntüle.Click += BtnGoruntule_Click;
                button4.Click += BtnYardim_Click;
                button5.Click += BtnCikis_Click;
                dataGridView1.CellClick += DataGridView1_CellClick;
                btngnrapor.Click += BtnGenelRapor_Click;
                btnsdkrapor.Click += BtnSdkRapor_Click;
                
                // Filtreleme için Enter tuşu desteği
                textBox1.KeyPress += (s, ec) => {
                    if (ec.KeyChar == (char)Keys.Enter)
                    {
                        button6_Click(null, null);
                        ec.Handled = true;
                    }
                };

                // Sol menü butonlarını bul ve event handler ekle
                foreach (Control ctrl in panel2.Controls)
                {
                    if (ctrl is Button btn)
                    {
                        switch (btn.Name)
                        {
                            case "button1": // Ürün Onay
                                btn.Click += BtnUrunOnay_Click;
                                break;
                            case "btnRaporlama": // Raporlama menüsü
                                btn.Click += BtnRaporlama_Click;
                                break;
                        }
                    }
                }

                // Filtreleme kontrollerini doldur
                LoadFiltreKontrolleri();

                // Verileri yükle
                LoadCiftlikler();

                // TextBox'ları readonly yap
                txtunvan.ReadOnly = true;
                txtvergi.ReadOnly = true;
                txtsektör.ReadOnly = true;
                txtadres.ReadOnly = true;
                txtbşdetay.ReadOnly = false;

                groupBox2.Text = "Çiftlik Bilgileri";
                groupBox1.Text = "Onay Bekleyen Çiftlikler";
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
                // Sektör ComboBox'ını doldur
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Tüm Sektörler");
                DataTable sektorler = DatabaseHelper.ExecuteQuery("SELECT DISTINCT SektorAdi FROM Tbl_Sektorler ORDER BY SektorAdi");
                foreach (DataRow row in sektorler.Rows)
                {
                    comboBox1.Items.Add(row["SektorAdi"].ToString());
                }
                comboBox1.SelectedIndex = 0;

                // Şehir ComboBox'ını doldur
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Tüm Şehirler");
                DataTable sehirler = DatabaseHelper.ExecuteQuery("SELECT DISTINCT SehirAdi FROM Tbl_Sehirler ORDER BY SehirAdi");
                foreach (DataRow row in sehirler.Rows)
                {
                    comboBox2.Items.Add(row["SehirAdi"].ToString());
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

        private void LoadCiftlikler(string aramaMetni = "", string sektor = "", string sehir = "")
        {
            try
            {
                string query = @"
                    SELECT 
                        c.CiftlikID,
                        c.Unvan as 'Çiftlik Ünvanı',
                        c.VergiNo as 'Vergi No',
                        s.SektorAdi as 'Sektör',
                        sh.SehirAdi as 'Şehir',
                        d.DurumAdi as 'Durum'
                    FROM Tbl_Ciftlikler c
                    LEFT JOIN Tbl_Sektorler s ON c.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON c.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON c.DurumID = d.DurumID
                    WHERE c.DurumID = 1";

                // Filtreleme koşulları
                if (!string.IsNullOrWhiteSpace(aramaMetni))
                {
                    query += $" AND (c.Unvan LIKE '%{aramaMetni}%' OR c.VergiNo LIKE '%{aramaMetni}%')";
                }

                if (!string.IsNullOrWhiteSpace(sektor) && sektor != "Tüm Sektörler")
                {
                    query += $" AND s.SektorAdi = '{sektor}'";
                }

                if (!string.IsNullOrWhiteSpace(sehir) && sehir != "Tüm Şehirler")
                {
                    query += $" AND sh.SehirAdi = '{sehir}'";
                }

                query += " ORDER BY c.CiftlikID DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["CiftlikID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                groupBox1.Text = $"Onay Bekleyen Çiftlikler ({dt.Rows.Count} adet)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çiftlikler yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                    selectedCiftlikID = Convert.ToInt32(row.Cells["CiftlikID"].Value);
                    LoadCiftlikDetay(selectedCiftlikID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seçim hatası: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCiftlikDetay(int ciftlikID)
        {
            try
            {
                string query = @"
                    SELECT 
                        c.Unvan, c.VergiNo, c.Adres,
                        s.SektorAdi
                    FROM Tbl_Ciftlikler c
                    LEFT JOIN Tbl_Sektorler s ON c.SektorID = s.SektorID
                    WHERE c.CiftlikID = @ciftlikID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@ciftlikID", ciftlikID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtunvan.Text = row["Unvan"]?.ToString() ?? "";
                    txtvergi.Text = row["VergiNo"]?.ToString() ?? "";
                    txtsektör.Text = row["SektorAdi"]?.ToString() ?? "";
                    txtadres.Text = row["Adres"]?.ToString() ?? "";
                    label3.Text = row["Unvan"]?.ToString() ?? "[Çiftlik Adı]";
                }

                LoadBelgeler(ciftlikID);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBelgeler(int ciftlikID)
        {
            try
            {
                listBelge.Items.Clear();
                listBelge.Tag = new Dictionary<string, string>(); // BelgeAdi -> DosyaYolu mapping

                string query = "SELECT BelgeAdi, DosyaYolu FROM Tbl_CiftlikBelgeleri WHERE CiftlikID = @ciftlikID";
                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@ciftlikID", ciftlikID));

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

        private void BtnOnayla_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedCiftlikID == 0)
                {
                    MessageBox.Show("Lütfen onaylanacak bir çiftlik seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{txtunvan.Text} çiftliğini onaylamak istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string updateQuery = "UPDATE Tbl_Ciftlikler SET DurumID = 2 WHERE CiftlikID = @ciftlikID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@ciftlikID", selectedCiftlikID));

                    if (affected > 0)
                    {
                        DatabaseHelper.LogIslem($"{txtunvan.Text} çiftliği {Session.KullaniciAdi} tarafından onaylandı");
                        MessageBox.Show("Çiftlik başarıyla onaylandı!", 
                            "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadCiftlikler();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Onaylama sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReddet_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedCiftlikID == 0)
                {
                    MessageBox.Show("Lütfen reddedilecek bir çiftlik seçin!", 
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
                    $"{txtunvan.Text} çiftliğini reddetmek istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string updateQuery = "UPDATE Tbl_Ciftlikler SET DurumID = 3 WHERE CiftlikID = @ciftlikID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@ciftlikID", selectedCiftlikID));

                    if (affected > 0)
                    {
                        DatabaseHelper.LogIslem($"{txtunvan.Text} çiftliği {Session.KullaniciAdi} tarafından reddedildi - Neden: {txtbşdetay.Text}");
                        MessageBox.Show("Çiftlik başvurusu reddedildi!", 
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadCiftlikler();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reddetme sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            selectedCiftlikID = 0;
            txtunvan.Clear();
            txtvergi.Clear();
            txtsektör.Clear();
            txtadres.Clear();
            txtbşdetay.Clear();
            listBelge.Items.Clear();
            listBelge.Tag = null;
            label3.Text = "[Seçili Çiftliğin Adı]";
        }

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

        private void button7_Click(object sender, EventArgs e)
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

        private void BtnGenelRapor_Click(object sender, EventArgs e)
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

        private void BtnSdkRapor_Click(object sender, EventArgs e)
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

        private void BtnUrunOnay_Click(object sender, EventArgs e)
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

        private void BtnRaporlama_Click(object sender, EventArgs e)
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

        /// <summary>
        /// Filtreleme butonu
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string aramaMetni = textBox1.Text.Trim();
                string sektor = comboBox1.SelectedItem?.ToString() ?? "";
                string sehir = comboBox2.SelectedItem?.ToString() ?? "";

                LoadCiftlikler(aramaMetni, sektor, sehir);
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
                LoadCiftlikler();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtre temizleme sırasında hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
