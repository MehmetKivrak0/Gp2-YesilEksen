using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using YesilEksen.Tarım;

namespace YesilEksen
{
    public partial class Login : Form
    {
        private bool isLoggedIn = false;

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Veritabanını başlat
            DatabaseHelper.InitializeDatabase();

            // Parola alanını gizli yap
            textBox1.PasswordChar = '*';
            textBox1.UseSystemPasswordChar = true;

            // Placeholder metinler
            textBox2.Text = "";
            textBox1.Text = "";

            // Form başlığını ayarla
            this.Text = "Yeşil Eksen - Giriş";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Event handler'ları bağla
            btngiris.Click += BtnGiris_Click;
            btnhakkimizda.Click += BtnHakkimizda_Click;
            btnyardim.Click += BtnYardim_Click;

            // Enter tuşu ile giriş
            textBox1.KeyPress += TextBox_KeyPress;
            textBox2.KeyPress += TextBox_KeyPress;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnGiris_Click(sender, e);
                e.Handled = true;
            }
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            try
            {
                // Boşluk kontrolü
                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Kullanıcı adı boş bırakılamaz!", "Uyarı", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox2.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Parola boş bırakılamaz!", "Uyarı", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                string kullaniciAdi = textBox2.Text.Trim();
                string parola = textBox1.Text.Trim();

                // Admin kullanıcılarının var olduğundan emin ol (giriş öncesi kontrol)
                DatabaseHelper.ResetAdminUsers();

                // Kısa bir bekleme (connection'ların kapanması için)
                System.Threading.Thread.Sleep(100);

                // Veritabanından kullanıcı kontrolü
                // Önce kullanıcıyı bul, sonra rol adını al
                string query = @"
                    SELECT k.KullaniciID, k.KullaniciAdi, k.RolID, 
                           COALESCE(r.RolAdi, 'Bilinmeyen') as RolAdi, 
                           k.IlgiliID, k.DurumID, k.SifreHash
                    FROM Tbl_Kullanicilar k
                    LEFT JOIN Tbl_Roller r ON k.RolID = r.RolID
                    WHERE k.KullaniciAdi = @kullaniciAdi AND k.SifreHash = @parola";

                DataTable dt = DatabaseHelper.ExecuteQuery(query,
                    new SQLiteParameter("@kullaniciAdi", kullaniciAdi),
                    new SQLiteParameter("@parola", parola));

                // Debug: Veritabanındaki tüm admin kullanıcılarını kontrol et
                DataTable debugDt = DatabaseHelper.ExecuteQuery(
                    "SELECT KullaniciAdi, SifreHash, RolID, DurumID FROM Tbl_Kullanicilar WHERE RolID IN (3, 4) OR KullaniciAdi IN ('sanayi_admin', 'ziraat_admin')");
                
                // Roller tablosunu kontrol et
                DataTable rollerDt = DatabaseHelper.ExecuteQuery("SELECT RolID, RolAdi FROM Tbl_Roller WHERE RolID IN (3, 4)");
                
                if (rollerDt.Rows.Count < 2)
                {
                    MessageBox.Show("Roller tablosunda admin rolleri bulunamadı! Veritabanı başlatılamadı.",
                        "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (debugDt.Rows.Count == 0)
                {
                    MessageBox.Show("Veritabanında admin kullanıcı bulunamadı! Lütfen uygulamayı yeniden başlatın.",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int durumID = Convert.ToInt32(row["DurumID"]);

                    // Hesap onay durumu kontrolü
                    if (durumID != 2)
                    {
                        MessageBox.Show("Hesabınız henüz onaylanmamış. Lütfen yönetici ile iletişime geçin.", 
                            "Erişim Engellendi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Session bilgilerini doldur
                    Session.KullaniciID = Convert.ToInt32(row["KullaniciID"]);
                    Session.KullaniciAdi = row["KullaniciAdi"].ToString();
                    Session.RolID = Convert.ToInt32(row["RolID"]);
                    Session.RolAdi = row["RolAdi"].ToString();
                    Session.IlgiliID = row["IlgiliID"] != DBNull.Value ? (int?)Convert.ToInt32(row["IlgiliID"]) : null;

                    // İşlem logla
                    DatabaseHelper.LogIslem($"{Session.KullaniciAdi} ({Session.RolAdi}) giriş yaptı");

                    MessageBox.Show($"Hoş geldiniz, {Session.KullaniciAdi}!\nRol: {Session.RolAdi}", 
                        "Giriş Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Rol'e göre yönlendirme
                    Form hedefForm = null;

                    switch (Session.RolID)
                    {
                        case 3: // Sanayi Odası Admin
                            hedefForm = new Form1(); // Sanayi Dashboard
                            break;
                        case 4: // Ziraat Odası Admin
                            hedefForm = new Çİftçi_Dasboard(); // Ziraat Dashboard
                            break;
                        case 1: // Firma
                            hedefForm = new Form1(); // Firma için de Sanayi Dashboard (kısıtlı)
                            break;
                        case 2: // Çiftlik
                            hedefForm = new Çİftçi_Dasboard(); // Çiftlik Dashboard
                            break;
                        default:
                            hedefForm = new Form1();
                            break;
                    }

                    isLoggedIn = true;
                    hedefForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya parola hatalı!", "Giriş Başarısız", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox1.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Giriş sırasında bir hata oluştu:\n{ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHakkimizda_Click(object sender, EventArgs e)
        {
            try
            {
                Hakkimizda hakkimizda = new Hakkimizda();
                hakkimizda.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Form kapatıldığında uygulamayı kapat (sadece giriş yapılmadıysa)
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!isLoggedIn && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Çıkış Yap butonu
        /// </summary>
        private void BtnCikis_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Uygulamadan çıkmak istediğinizden emin misiniz?", 
                    "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çıkış yapılırken hata: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tüm Login formlarını kapatır ve yeni bir Login formu oluşturup gösterir
        /// </summary>
        public static void ShowLoginForm()
        {
            // Tüm açık Login formlarını kapat (gizli olsa bile)
            var loginForms = new List<Login>();
            foreach (Form form in Application.OpenForms)
            {
                if (form is Login loginForm)
                {
                    loginForms.Add(loginForm);
                }
            }
            
            // Tüm Login formlarını kapat
            foreach (var loginForm in loginForms)
            {
                loginForm.Close();
            }
            
            // Yeni Login formu oluştur ve göster
            Login newLogin = new Login();
            newLogin.Show();
            newLogin.WindowState = FormWindowState.Normal;
            newLogin.BringToFront();
        }
    }
}
