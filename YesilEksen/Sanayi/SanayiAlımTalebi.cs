using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using YesilEksen.Sanayi;

namespace YesilEksen
{
    public partial class SanayiAlımTalebi : Form
    {
        private int selectedTalepID = 0;

        public SanayiAlımTalebi()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Başlık click eventi
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
        }

        private void btnanasayfa_Click(object sender, EventArgs e)
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

        private void button3_Click(object sender, EventArgs e)
        {
            // Reddet butonu - event handler aşağıda
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiFirmaOnay sanayiFirma = new SanayiFirmaOnay();
                sanayiFirma.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa açılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SanayiAlımTalebi_Load(object sender, EventArgs e)
        {
            try
            {
                // Event handler'ları bağla
                button1.Click += BtnOnayla_Click;
                button3.Click += BtnReddet_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                btnYardım.Click += BtnYardim_Click;
                dataGridView1.CellClick += DataGridView1_CellClick;

                // Verileri yükle
                LoadTalepler();

                // TextBox'ları readonly yap (reddetme nedeni hariç)
                textBox2.ReadOnly = true; // Firma
                textBox3.ReadOnly = true; // Çiftlik
                textBox4.ReadOnly = true; // Ürün
                textBox5.ReadOnly = true; // Miktar
                textBox6.ReadOnly = true; // Firma notu
                textBox1.ReadOnly = false; // Reddetme nedeni
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Onay bekleyen talepleri yükler
        /// </summary>
        private void LoadTalepler()
        {
            try
            {
                string query = @"
                    SELECT 
                        t.TalepID,
                        f.Unvan as 'Talep Eden Firma',
                        c.Unvan as 'Hedef Çiftlik',
                        u.UrunAdi as 'Ürün',
                        t.TalepMiktarTon as 'Miktar (Ton)',
                        d.DurumAdi as 'Durum',
                        t.TalepTarihi as 'Talep Tarihi'
                    FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_Firmalar f ON t.FirmaID = f.FirmaID
                    LEFT JOIN Tbl_Ciftlikler c ON t.HedefCiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_CiftlikUrunleri u ON t.UrunID = u.UrunID
                    LEFT JOIN Tbl_OnayDurumlari d ON t.DurumID = d.DurumID
                    WHERE t.DurumID = 1
                    ORDER BY t.TalepTarihi DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                // Grid görünümünü düzenle
                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["TalepID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                // GroupBox başlığını güncelle
                groupBox1.Text = $"Onay Bekleyen Talepler ({dt.Rows.Count} adet)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Talepler yüklenirken hata: {ex.Message}", 
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
                    selectedTalepID = Convert.ToInt32(row.Cells["TalepID"].Value);

                    // Talep detaylarını göster
                    LoadTalepDetay(selectedTalepID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seçim hatası: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Seçili talebin detaylarını yükler
        /// </summary>
        private void LoadTalepDetay(int talepID)
        {
            try
            {
                string query = @"
                    SELECT 
                        t.TalepID, t.FirmaNotu, t.ReddetmeNedeni, t.TalepMiktarTon,
                        f.Unvan as FirmaUnvan, f.DurumID as FirmaDurum,
                        c.Unvan as CiftlikUnvan, c.DurumID as CiftlikDurum,
                        u.UrunAdi,
                        fd.DurumAdi as FirmaDurumAdi,
                        cd.DurumAdi as CiftlikDurumAdi
                    FROM Tbl_AlimTalepleri t
                    LEFT JOIN Tbl_Firmalar f ON t.FirmaID = f.FirmaID
                    LEFT JOIN Tbl_Ciftlikler c ON t.HedefCiftlikID = c.CiftlikID
                    LEFT JOIN Tbl_CiftlikUrunleri u ON t.UrunID = u.UrunID
                    LEFT JOIN Tbl_OnayDurumlari fd ON f.DurumID = fd.DurumID
                    LEFT JOIN Tbl_OnayDurumlari cd ON c.DurumID = cd.DurumID
                    WHERE t.TalepID = @talepID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@talepID", talepID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    textBox2.Text = row["FirmaUnvan"]?.ToString() ?? "";
                    textBox3.Text = row["CiftlikUnvan"]?.ToString() ?? "";
                    textBox4.Text = row["UrunAdi"]?.ToString() ?? "";
                    textBox5.Text = row["TalepMiktarTon"]?.ToString() + " Ton" ?? "";
                    textBox6.Text = row["FirmaNotu"]?.ToString() ?? "";
                    textBox1.Text = row["ReddetmeNedeni"]?.ToString() ?? "";

                    // Durum labellarını güncelle
                    string firmaDurum = row["FirmaDurumAdi"]?.ToString() ?? "";
                    string ciftlikDurum = row["CiftlikDurumAdi"]?.ToString() ?? "";
                    
                    label8.Text = $"Durumu > {firmaDurum}";
                    label8.ForeColor = firmaDurum == "Onaylandı" ? Color.Green : Color.Red;
                    
                    label9.Text = $"Durumu > {ciftlikDurum}";
                    label9.ForeColor = ciftlikDurum == "Onaylandı" ? Color.Green : Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Talebi onaylar
        /// </summary>
        private void BtnOnayla_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedTalepID == 0)
                {
                    MessageBox.Show("Lütfen onaylanacak bir talep seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{textBox2.Text} firmasının {textBox4.Text} talebini onaylamak istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Talebi onayla (DurumID = 2)
                    string updateQuery = "UPDATE Tbl_AlimTalepleri SET DurumID = 2 WHERE TalepID = @talepID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@talepID", selectedTalepID));

                    if (affected > 0)
                    {
                        // SDG Rapor verisini ekle (sürdürülebilirlik etkisi)
                        double miktar = 0;
                        double.TryParse(textBox5.Text.Replace(" Ton", ""), out miktar);
                        
                        // Tahmini CO2 ve ekonomik değer hesapla
                        double engellenenCO2 = miktar * 0.45; // 1 ton atık = 0.45 ton CO2 tasarrufu
                        double ekonomikDeger = miktar * 1250; // 1 ton = 1250 TL ekonomik değer

                        string sdgQuery = @"INSERT INTO Tbl_SdgRaporVerisi 
                            (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) 
                            VALUES (@talepID, @atik, @co2, @ekonomik)";
                        
                        DatabaseHelper.ExecuteNonQuery(sdgQuery,
                            new SQLiteParameter("@talepID", selectedTalepID),
                            new SQLiteParameter("@atik", miktar),
                            new SQLiteParameter("@co2", engellenenCO2),
                            new SQLiteParameter("@ekonomik", ekonomikDeger));

                        // Log kaydet
                        DatabaseHelper.LogIslem(
                            $"{textBox2.Text} firmasının {textBox4.Text} talebi ({miktar} Ton) {Session.KullaniciAdi} tarafından onaylandı");

                        MessageBox.Show($"Talep başarıyla onaylandı!\n\nSürdürülebilirlik Etkisi:\n" +
                            $"• Geri Kazanılan Atık: {miktar:N2} Ton\n" +
                            $"• Engellenen CO2: {engellenenCO2:N2} Ton\n" +
                            $"• Ekonomik Değer: {ekonomikDeger:N2} TL", 
                            "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Formu temizle ve verileri yenile
                        ClearForm();
                        LoadTalepler();
                    }
                    else
                    {
                        MessageBox.Show("Talep onaylanamadı!", 
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
        /// Talebi reddeder
        /// </summary>
        private void BtnReddet_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedTalepID == 0)
                {
                    MessageBox.Show("Lütfen reddedilecek bir talep seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Lütfen red nedenini yazın!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{textBox2.Text} firmasının talebini reddetmek istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Talebi reddet (DurumID = 3)
                    string updateQuery = @"UPDATE Tbl_AlimTalepleri 
                        SET DurumID = 3, ReddetmeNedeni = @neden 
                        WHERE TalepID = @talepID";
                    
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@talepID", selectedTalepID),
                        new SQLiteParameter("@neden", textBox1.Text));

                    if (affected > 0)
                    {
                        // Log kaydet
                        DatabaseHelper.LogIslem(
                            $"{textBox2.Text} firmasının {textBox4.Text} talebi {Session.KullaniciAdi} tarafından reddedildi - Neden: {textBox1.Text}");

                        MessageBox.Show("Talep reddedildi!", 
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Formu temizle ve verileri yenile
                        ClearForm();
                        LoadTalepler();
                    }
                    else
                    {
                        MessageBox.Show("Talep reddedilemedi!", 
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
        /// Formu temizler
        /// </summary>
        private void ClearForm()
        {
            selectedTalepID = 0;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            label8.Text = "Durumu >";
            label9.Text = "Durumu >";
            label8.ForeColor = SystemColors.ControlText;
            label9.ForeColor = SystemColors.ControlText;
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

        private void btngnrapor_Click(object sender, EventArgs e)
        {
            try
            {
                Sanayi.SanayiGenelRapor sanayiGenelRapor = new Sanayi.SanayiGenelRapor();
                sanayiGenelRapor.Show();
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

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
