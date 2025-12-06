using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using YesilEksen.Sanayi;

namespace YesilEksen
{
    public partial class SanayiFirmaOnay : Form
    {
        private int selectedFirmaID = 0;

        public SanayiFirmaOnay()
        {
            InitializeComponent();
        }

        private void SanayiFirmaOnay_Load(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                // Event handler'ları bağla
                btnOnayla.Click += BtnOnayla_Click;
                btnReddet.Click += BtnReddet_Click;
                btnGörüntüle.Click += BtnGoruntule_Click;
                btnYardım.Click += BtnYardim_Click;
                btnÇıkışYap.Click += BtnCikis_Click;
                dataGridView1.CellClick += DataGridView1_CellClick;

                // Verileri yükle
                LoadFirmalar();

                // TextBox'ları readonly yap (reddetme nedeni hariç)
                txtunvan.ReadOnly = true;
                txtvergi.ReadOnly = true;
                txtsektör.ReadOnly = true;
                txtadres.ReadOnly = true;
                txtbşdetay.ReadOnly = false;

                groupBox2.Text = "Firma Bilgileri";
                groupBox1.Text = "Onay Bekleyen Firmalar";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Onay bekleyen firmaları yükler
        /// </summary>
        private void LoadFirmalar()
        {
            try
            {
                string query = @"
                    SELECT 
                        f.FirmaID,
                        f.Unvan as 'Firma Ünvanı',
                        f.VergiNo as 'Vergi No',
                        s.SektorAdi as 'Sektör',
                        sh.SehirAdi as 'Şehir',
                        d.DurumAdi as 'Durum'
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    LEFT JOIN Tbl_Sehirler sh ON f.SehirID = sh.SehirID
                    LEFT JOIN Tbl_OnayDurumlari d ON f.DurumID = d.DurumID
                    WHERE f.DurumID = 1
                    ORDER BY f.FirmaID DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = dt;

                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["FirmaID"].Visible = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                groupBox1.Text = $"Onay Bekleyen Firmalar ({dt.Rows.Count} adet)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firmalar yüklenirken hata: {ex.Message}", 
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
                    selectedFirmaID = Convert.ToInt32(row.Cells["FirmaID"].Value);
                    LoadFirmaDetay(selectedFirmaID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seçim hatası: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Seçili firmanın detaylarını yükler
        /// </summary>
        private void LoadFirmaDetay(int firmaID)
        {
            try
            {
                string query = @"
                    SELECT 
                        f.Unvan, f.VergiNo, f.Adres,
                        s.SektorAdi
                    FROM Tbl_Firmalar f
                    LEFT JOIN Tbl_Sektorler s ON f.SektorID = s.SektorID
                    WHERE f.FirmaID = @firmaID";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@firmaID", firmaID));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtunvan.Text = row["Unvan"]?.ToString() ?? "";
                    txtvergi.Text = row["VergiNo"]?.ToString() ?? "";
                    txtsektör.Text = row["SektorAdi"]?.ToString() ?? "";
                    txtadres.Text = row["Adres"]?.ToString() ?? "";
                    label3.Text = row["Unvan"]?.ToString() ?? "[Firma Adı]";
                }

                // Belgeleri yükle
                LoadBelgeler(firmaID);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay yüklenirken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Firma belgelerini yükler
        /// </summary>
        private void LoadBelgeler(int firmaID)
        {
            try
            {
                listBelge.Items.Clear();

                string query = "SELECT BelgeAdi FROM Tbl_FirmaBelgeleri WHERE FirmaID = @firmaID";
                DataTable dt = DatabaseHelper.ExecuteQuery(query, 
                    new SQLiteParameter("@firmaID", firmaID));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        listBelge.Items.Add(row["BelgeAdi"].ToString());
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
        /// Firmayı onaylar
        /// </summary>
        private void BtnOnayla_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedFirmaID == 0)
                {
                    MessageBox.Show("Lütfen onaylanacak bir firma seçin!", 
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{txtunvan.Text} firmasını onaylamak istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Firmayı onayla (DurumID = 2)
                    string updateQuery = "UPDATE Tbl_Firmalar SET DurumID = 2 WHERE FirmaID = @firmaID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@firmaID", selectedFirmaID));

                    if (affected > 0)
                    {
                        // Log kaydet
                        DatabaseHelper.LogIslem($"{txtunvan.Text} firması {Session.KullaniciAdi} tarafından onaylandı");

                        MessageBox.Show("Firma başarıyla onaylandı!", 
                            "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadFirmalar();
                    }
                    else
                    {
                        MessageBox.Show("Firma onaylanamadı!", 
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
        /// Firmayı reddeder
        /// </summary>
        private void BtnReddet_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedFirmaID == 0)
                {
                    MessageBox.Show("Lütfen reddedilecek bir firma seçin!", 
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
                    $"{txtunvan.Text} firmasını reddetmek istediğinizden emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Firmayı reddet (DurumID = 3)
                    string updateQuery = "UPDATE Tbl_Firmalar SET DurumID = 3 WHERE FirmaID = @firmaID";
                    int affected = DatabaseHelper.ExecuteNonQuery(updateQuery, 
                        new SQLiteParameter("@firmaID", selectedFirmaID));

                    if (affected > 0)
                    {
                        // Log kaydet
                        DatabaseHelper.LogIslem($"{txtunvan.Text} firması {Session.KullaniciAdi} tarafından reddedildi - Neden: {txtbşdetay.Text}");

                        MessageBox.Show("Firma başvurusu reddedildi!", 
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                        LoadFirmalar();
                    }
                    else
                    {
                        MessageBox.Show("Firma reddedilemedi!", 
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
            if (listBelge.SelectedItem == null || listBelge.SelectedItem.ToString() == "Belge bulunamadı")
            {
                MessageBox.Show("Lütfen görüntülenecek bir belge seçin!", 
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"'{listBelge.SelectedItem}' belgesi görüntüleniyor...", 
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Formu temizler
        /// </summary>
        private void ClearForm()
        {
            selectedFirmaID = 0;
            txtunvan.Clear();
            txtvergi.Clear();
            txtsektör.Clear();
            txtadres.Clear();
            txtbşdetay.Clear();
            listBelge.Items.Clear();
            label3.Text = "[Seçili Firmanın Adı]";
        }

        private void btnRaporlama_Click(object sender, EventArgs e)
        {
            pnlAltMenu.Visible = !pnlAltMenu.Visible;
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

        private void btngnrapor_Click(object sender, EventArgs e)
        {
            try
            {
                SanayiGenelRapor sanayiGenelRapor = new SanayiGenelRapor();
                sanayiGenelRapor.Show();
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
                Tarım.SdkRapor sdkRapor = new Tarım.SdkRapor();
                sdkRapor.Show();
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

        private void btnÇıkışYap_Click(object sender, EventArgs e)
        {

        }
    }
}
