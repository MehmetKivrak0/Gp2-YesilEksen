using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace YesilEksen
{
    /// <summary>
    /// Veritabanı işlemlerini yöneten yardımcı sınıf
    /// </summary>
    public static class DatabaseHelper
    {
        // Veritabanı dosyasının yolu
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YesilEksen.db");

        // Connection string (WAL modu ile daha iyi eşzamanlı erişim, timeout artırıldı)
        private static string ConnectionString => $"Data Source={DbPath};Version=3;Journal Mode=WAL;Busy Timeout=30000;";

        /// <summary>
        /// Yeni bir SQLite bağlantısı döndürür
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        /// <summary>
        /// Veritabanının var olup olmadığını kontrol eder, yoksa oluşturur
        /// </summary>
        public static void InitializeDatabase()
        {
            try
            {
                bool isNewDb = !File.Exists(DbPath);

                if (isNewDb)
                {
                    SQLiteConnection.CreateFile(DbPath);
                    CreateTables();
                    MessageBox.Show("Veritabanı oluşturuldu!",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Veritabanı var, tabloları kontrol et
                    CreateTables();
                    
                    // Temel verileri garanti et (Roller, OnayDurumlari)
                    EnsureBasicData();
                    
                    // Admin kullanıcılarını garanti et
                    EnsureAdminUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı başlatılırken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Admin kullanıcılarını siler ve yeniden ekler (güvenli yöntem)
        /// </summary>
        public static void ResetAdminUsers()
        {
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    using (var conn = GetConnection())
                    {
                        conn.Open();

                        // Önce mevcut admin kullanıcılarını sil
                        using (var deleteCmd = new SQLiteCommand(
                            "DELETE FROM Tbl_Kullanicilar WHERE RolID IN (3, 4) OR KullaniciAdi IN ('sanayi_admin', 'ziraat_admin')",
                            conn))
                        {
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Kısa bir bekleme
                        System.Threading.Thread.Sleep(50);

                        // Sanayi admin ekle
                        using (var insertCmd = new SQLiteCommand(
                            "INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (3, 'sanayi_admin', '123456', NULL, 2)",
                            conn))
                        {
                            insertCmd.ExecuteNonQuery();
                        }

                        // Ziraat admin ekle
                        using (var insertCmd = new SQLiteCommand(
                            "INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (4, 'ziraat_admin', '123456', NULL, 2)",
                            conn))
                        {
                            insertCmd.ExecuteNonQuery();
                        }

                        // Eklendiğini doğrula
                        using (var verifyCmd = new SQLiteCommand(
                            "SELECT COUNT(*) FROM Tbl_Kullanicilar WHERE KullaniciAdi IN ('sanayi_admin', 'ziraat_admin')",
                            conn))
                        {
                            object result = verifyCmd.ExecuteScalar();
                            int count = result != null ? Convert.ToInt32(result) : 0;
                            
                            if (count == 2)
                            {
                                // Başarılı, döngüden çık
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        MessageBox.Show($"Admin kullanıcıları sıfırlanırken hata: {ex.Message}",
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                    // Kısa bir bekleme sonrası tekrar dene
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Temel verileri (Roller, OnayDurumlari) garanti eder
        /// </summary>
        private static void EnsureBasicData()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    // Roller
                    string[] rollerSQL = {
                        "INSERT OR IGNORE INTO Tbl_Roller (RolID, RolAdi) VALUES (1, 'Firma')",
                        "INSERT OR IGNORE INTO Tbl_Roller (RolID, RolAdi) VALUES (2, 'Ciftlik')",
                        "INSERT OR IGNORE INTO Tbl_Roller (RolID, RolAdi) VALUES (3, 'Sanayi Odası Admin')",
                        "INSERT OR IGNORE INTO Tbl_Roller (RolID, RolAdi) VALUES (4, 'Ziraat Odası Admin')"
                    };

                    foreach (string sql in rollerSQL)
                    {
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Onay Durumları
                    string[] durumlarSQL = {
                        "INSERT OR IGNORE INTO Tbl_OnayDurumlari (DurumID, DurumAdi) VALUES (1, 'Onay Bekliyor')",
                        "INSERT OR IGNORE INTO Tbl_OnayDurumlari (DurumID, DurumAdi) VALUES (2, 'Onaylandı')",
                        "INSERT OR IGNORE INTO Tbl_OnayDurumlari (DurumID, DurumAdi) VALUES (3, 'Reddedildi')"
                    };

                    foreach (string sql in durumlarSQL)
                    {
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et
            }
        }

        /// <summary>
        /// Admin kullanıcılarının var olduğundan emin olur, yoksa ekler
        /// </summary>
        public static void EnsureAdminUsers()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    // Sanayi admin kontrolü
                    using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Tbl_Kullanicilar WHERE KullaniciAdi = 'sanayi_admin'", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int count = result != null ? Convert.ToInt32(result) : 0;

                        if (count == 0)
                        {
                            using (var insertCmd = new SQLiteCommand(
                                "INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (3, 'sanayi_admin', '123456', NULL, 2)",
                                conn))
                            {
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Ziraat admin kontrolü
                    using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Tbl_Kullanicilar WHERE KullaniciAdi = 'ziraat_admin'", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int count = result != null ? Convert.ToInt32(result) : 0;

                        if (count == 0)
                        {
                            using (var insertCmd = new SQLiteCommand(
                                "INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (4, 'ziraat_admin', '123456', NULL, 2)",
                                conn))
                            {
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda sessizce devam et
            }
        }

        /// <summary>
        /// Veritabanını tamamen sıfırlar (tüm verileri siler ve tabloları yeniden oluşturur)
        /// </summary>
        public static void ResetDatabase()
        {
            try
            {
                // Tüm açık bağlantıları kapat
                SQLiteConnection.ClearAllPools();
                
                // Garbage collection yaparak bağlantıları serbest bırak
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Bağlantıların kapanması için bekleme
                System.Threading.Thread.Sleep(1000);

                // Veritabanı dosyasını silmek yerine, içeriğini temizle
                // Bu yaklaşım dosya kilitleme sorunlarını önler
                if (File.Exists(DbPath))
                {
                    try
                    {
                        // Önce tüm tabloları DROP et (dosyayı silmeden)
                        using (var conn = GetConnection())
                        {
                            conn.Open();
                            
                            // Tüm tabloları sil
                            string[] dropTables = {
                                "DROP TABLE IF EXISTS Tbl_IslemLoglari",
                                "DROP TABLE IF EXISTS Tbl_SdgRaporVerisi",
                                "DROP TABLE IF EXISTS Tbl_UrunBelgeleri",
                                "DROP TABLE IF EXISTS Tbl_FirmaBelgeleri",
                                "DROP TABLE IF EXISTS Tbl_CiftlikBelgeleri",
                                "DROP TABLE IF EXISTS Tbl_AlimTalepleri",
                                "DROP TABLE IF EXISTS Tbl_CiftlikUrunleri",
                                "DROP TABLE IF EXISTS Tbl_Kullanicilar",
                                "DROP TABLE IF EXISTS Tbl_Ciftlikler",
                                "DROP TABLE IF EXISTS Tbl_Firmalar",
                                "DROP TABLE IF EXISTS Tbl_Roller",
                                "DROP TABLE IF EXISTS Tbl_OnayDurumlari",
                                "DROP TABLE IF EXISTS Tbl_UrunKategorileri",
                                "DROP TABLE IF EXISTS Tbl_Sektorler",
                                "DROP TABLE IF EXISTS Tbl_Sehirler"
                            };
                            
                            foreach (string dropSql in dropTables)
                            {
                                try
                                {
                                    using (var cmd = new SQLiteCommand(dropSql, conn))
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch { } // Tablo yoksa hata verme
                            }
                            
                            // VACUUM ile veritabanını temizle
                            using (var cmd = new SQLiteCommand("VACUUM", conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            
                            conn.Close();
                        }
                        
                        // Bağlantıyı kapat ve bekle
                        SQLiteConnection.ClearAllPools();
                        GC.Collect();
                        System.Threading.Thread.Sleep(500);
                        
                        // Şimdi dosyayı silmeyi dene (artık kullanılmıyor olmalı)
                        int retryCount = 0;
                        int maxRetries = 5;
                        bool deleted = false;
                        
                        while (retryCount < maxRetries && !deleted)
                        {
                            try
                            {
                                File.SetAttributes(DbPath, FileAttributes.Normal);
                                File.Delete(DbPath);
                                deleted = true;
                            }
                            catch
                            {
                                retryCount++;
                                if (retryCount < maxRetries)
                                {
                                    System.Threading.Thread.Sleep(200 * retryCount);
                                    SQLiteConnection.ClearAllPools();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Dosya silme başarısız olsa bile devam et
                        // Yeni dosya oluşturulurken eski dosya üzerine yazılacak
                    }
                }

                // WAL ve SHM dosyalarını da sil
                string walPath = DbPath + "-wal";
                string shmPath = DbPath + "-shm";
                string journalPath = DbPath + "-journal";
                
                string[] filesToDelete = { walPath, shmPath, journalPath };
                foreach (string filePath in filesToDelete)
                {
                    if (File.Exists(filePath))
                    {
                        try 
                        { 
                            File.SetAttributes(filePath, FileAttributes.Normal);
                            File.Delete(filePath); 
                        } 
                        catch { }
                    }
                }

                // Dosya silme işlemlerinin tamamlanması için bekleme
                System.Threading.Thread.Sleep(500);

                // Dosya yoksa oluştur, varsa zaten temizlenmiş (tablolar DROP edilmiş)
                if (!File.Exists(DbPath))
                {
                    SQLiteConnection.CreateFile(DbPath);
                }
                else
                {
                    // Dosya varsa ama tablolar silinmiş olabilir, tekrar kontrol et
                    // Eğer tablolar hala varsa (silme başarısız olduysa), tekrar DROP et
                    try
                    {
                        using (var conn = GetConnection())
                        {
                            conn.Open();
                            
                            // Tabloların var olup olmadığını kontrol et
                            using (var cmd = new SQLiteCommand(
                                "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'Tbl_%'", conn))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        // Hala tablolar varsa, tekrar DROP et
                                        string[] dropTables = {
                                            "DROP TABLE IF EXISTS Tbl_IslemLoglari",
                                            "DROP TABLE IF EXISTS Tbl_SdgRaporVerisi",
                                            "DROP TABLE IF EXISTS Tbl_UrunBelgeleri",
                                            "DROP TABLE IF EXISTS Tbl_FirmaBelgeleri",
                                            "DROP TABLE IF EXISTS Tbl_CiftlikBelgeleri",
                                            "DROP TABLE IF EXISTS Tbl_AlimTalepleri",
                                            "DROP TABLE IF EXISTS Tbl_CiftlikUrunleri",
                                            "DROP TABLE IF EXISTS Tbl_Kullanicilar",
                                            "DROP TABLE IF EXISTS Tbl_Ciftlikler",
                                            "DROP TABLE IF EXISTS Tbl_Firmalar",
                                            "DROP TABLE IF EXISTS Tbl_Roller",
                                            "DROP TABLE IF EXISTS Tbl_OnayDurumlari",
                                            "DROP TABLE IF EXISTS Tbl_UrunKategorileri",
                                            "DROP TABLE IF EXISTS Tbl_Sektorler",
                                            "DROP TABLE IF EXISTS Tbl_Sehirler"
                                        };
                                        
                                        foreach (string dropSql in dropTables)
                                        {
                                            try
                                            {
                                                using (var dropCmd = new SQLiteCommand(dropSql, conn))
                                                {
                                                    dropCmd.ExecuteNonQuery();
                                                }
                                            }
                                            catch { }
                                        }
                                        
                                        // VACUUM yap
                                        using (var vacuumCmd = new SQLiteCommand("VACUUM", conn))
                                        {
                                            vacuumCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            
                            conn.Close();
                        }
                    }
                    catch { } // Hata olsa bile devam et
                }

                // Tabloları oluştur
                CreateTables();

                // Temel verileri ekle (Roller, OnayDurumlari)
                EnsureBasicData();

                // Admin kullanıcılarını ekle
                EnsureAdminUsers();

                MessageBox.Show("Veritabanı tamamen sıfırlandı ve yeniden oluşturuldu!",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı sıfırlanırken hata oluştu: {ex.Message}\n\nDetay: {ex.StackTrace}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Veritabanını tamamen yeniden oluşturur ve sentetik verileri ekler
        /// </summary>
        public static void RecreateDatabaseWithData()
        {
            try
            {
                var result = MessageBox.Show(
                    "Veritabanı tamamen silinecek ve yeniden oluşturulacak. Tüm mevcut veriler kaybolacak!\n\n" +
                    "Devam etmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Veritabanını sıfırla
                ResetDatabase();

                // Kısa bir bekleme
                System.Threading.Thread.Sleep(300);

                // Sentetik verileri ekle
                InsertSyntheticData();

                MessageBox.Show("Veritabanı başarıyla yeniden oluşturuldu ve tüm veriler eklendi!",
                    "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı yeniden oluşturulurken hata oluştu: {ex.Message}\n\nDetay: {ex.StackTrace}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ayrıntılı sentetik verileri ekler (her tabloda minimum 50 veri)
        /// </summary>
        public static void InsertSyntheticData()
        {
            try
            {
                // Önce kısa bir bekleme
                System.Threading.Thread.Sleep(100);

                // Belgeler klasöründeki dosyaları al
                string belgelerKlasoru = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Belgeler");
                string[] mevcutBelgeler = new string[0];
                
                if (Directory.Exists(belgelerKlasoru))
                {
                    mevcutBelgeler = Directory.GetFiles(belgelerKlasoru, "*.pdf");
                    // Sadece dosya adlarını al (klasör yolu olmadan)
                    for (int i = 0; i < mevcutBelgeler.Length; i++)
                    {
                        mevcutBelgeler[i] = Path.GetFileName(mevcutBelgeler[i]);
                    }
                }

                // Eğer belge yoksa varsayılan belgeler kullan
                if (mevcutBelgeler.Length == 0)
                {
                    mevcutBelgeler = new string[] 
                    { 
                        "sanayi-genel-rapor-2025-12-10.pdf",
                        "10295103416_Ogrenci (1).pdf",
                        "Internship2.pdf",
                        "Faaliyet Belgesi (1).pdf"
                    };
                }

                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Transaction başlat - tüm işlemler tek transaction içinde
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            Random rnd = new Random();
                            int eklenenSayisi = 0;
                            int toplamEklenen = 0;

                            // Önce mevcut verileri temizle (opsiyonel - sadece yeni veriler eklemek için)
                            // Bu kısım isteğe bağlı, eğer mevcut verileri korumak istiyorsanız burayı yorum satırı yapın
                            /*
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_SdgRaporVerisi", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_IslemLoglari", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_UrunBelgeleri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_FirmaBelgeleri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_CiftlikBelgeleri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_AlimTalepleri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_CiftlikUrunleri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_Kullanicilar WHERE RolID IN (1,2)", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_Ciftlikler", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_Firmalar", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_UrunKategorileri", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_Sektorler", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            using (var cmd = new SQLiteCommand("DELETE FROM Tbl_Sehirler", conn, transaction)) { cmd.ExecuteNonQuery(); }
                            */

                            // Önce mevcut verileri temizle (opsiyonel - sadece yeni veriler eklemek için)
                            // Bu satırı kaldırabilirsiniz, INSERT OR IGNORE kullanıyoruz zaten

                            // 1. Şehirler (50+ şehir)
                    string[] sehirler = {
                        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin",
                        "Aydın", "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa",
                        "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Edirne", "Elazığ", "Erzincan",
                        "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Isparta",
                        "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
                        "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla",
                        "Muş", "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop",
                        "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van",
                        "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale", "Batman", "Şırnak",
                        "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
                    };

                    foreach (string sehir in sehirler)
                    {
                        try
                        {
                            using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO Tbl_Sehirler (SehirAdi) VALUES (@sehir)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@sehir", sehir);
                                int result = cmd.ExecuteNonQuery();
                                if (result > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Hata olsa bile devam et
                            System.Diagnostics.Debug.WriteLine($"Şehir eklenirken hata: {sehir} - {ex.Message}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Şehirler eklendi: {eklenenSayisi}/{sehirler.Length}");
                    eklenenSayisi = 0;

                    // 2. Sektörler (50+ sektör)
                    string[] sektorler = {
                        "Tarım", "Sanayi", "İlaç", "Enerji", "Gıda", "Tekstil", "Kimya", "Ambalaj",
                        "Otomotiv", "İnşaat", "Teknoloji", "Turizm", "Eğitim", "Sağlık", "Finans", "Lojistik",
                        "Medya", "Telekomünikasyon", "Perakende", "Havacılık", "Denizcilik", "Demir Çelik",
                        "Petrol", "Doğalgaz", "Maden", "Orman Ürünleri", "Hayvancılık", "Balıkçılık",
                        "Elektrik", "Elektronik", "Makine", "Metal İşleme", "Plastik", "Cam", "Seramik",
                        "Kağıt", "Matbaa", "Mobilya", "Ayakkabı", "Deri", "Kozmetik", "Temizlik",
                        "Gübre", "Pestisit", "Tohum", "Yem", "Süt Ürünleri", "Et Ürünleri", "Unlu Mamuller",
                        "İçecek", "Şekerleme", "Baharat", "Organik Ürünler", "Biyokütle", "Yenilenebilir Enerji"
                    };

                    foreach (string sektor in sektorler)
                    {
                        try
                        {
                            using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO Tbl_Sektorler (SektorAdi) VALUES (@sektor)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@sektor", sektor);
                                int result = cmd.ExecuteNonQuery();
                                if (result > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Sektör eklenirken hata: {sektor} - {ex.Message}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Sektörler eklendi: {eklenenSayisi}/{sektorler.Length}");
                    eklenenSayisi = 0;

                    // 3. Ürün Kategorileri (50+ kategori)
                    string[] kategoriler = {
                        "Organik Atık", "Plastik Atık", "Metal Atık", "Cam Atık", "Kağıt Atık", "Biyokütle",
                        "Tarımsal Atık", "Gıda Atığı", "Bahçe Atığı", "Hayvansal Atık", "Endüstriyel Atık",
                        "Tıbbi Atık", "Elektronik Atık", "Tekstil Atığı", "Ahşap Atık", "Kompost",
                        "Tahıl Samanı", "Sebze Atığı", "Meyve Posası", "Zeytin Karasuyu", "Bağ Atığı",
                        "Süt Atığı", "Hayvan Gübresi", "Yem Artığı", "Bitki Sapı", "Mısır Koçanı",
                        "Ayçiçeği Sapı", "Pamuk Atığı", "Şeker Pancarı Yaprağı", "Domates Sapı",
                        "Pirinç Kabuğu", "Buğday Samanı", "Arpa Samanı", "Yulaf Samanı", "Çavdar Samanı",
                        "Mısır Sapı", "Soya Atığı", "Kanola Atığı", "Ayçiçeği Sapı", "Pamuk Çiğidi",
                        "Zeytin Prinası", "Fındık Kabuğu", "Ceviz Kabuğu", "Badem Kabuğu", "Çay Atığı",
                        "Kahve Atığı", "Şeker Kamışı Atığı", "Bambu Atığı", "Yosun", "Alg"
                    };

                    foreach (string kategori in kategoriler)
                    {
                        try
                        {
                            using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO Tbl_UrunKategorileri (KategoriAdi) VALUES (@kategori)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@kategori", kategori);
                                int result = cmd.ExecuteNonQuery();
                                if (result > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Kategori eklenirken hata: {kategori} - {ex.Message}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Kategoriler eklendi: {eklenenSayisi}/{kategoriler.Length}");
                    eklenenSayisi = 0;

                    // 4. Onay Durumları
                    string[] durumlar = { "Onay Bekliyor", "Onaylandı", "Reddedildi" };
                    for (int i = 0; i < durumlar.Length; i++)
                    {
                        using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO Tbl_OnayDurumlari (DurumID, DurumAdi) VALUES (@id, @durum)", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", i + 1);
                            cmd.Parameters.AddWithValue("@durum", durumlar[i]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 5. Roller
                    string[] roller = { "Firma", "Ciftlik", "Sanayi Odası Admin", "Ziraat Odası Admin" };
                    for (int i = 0; i < roller.Length; i++)
                    {
                        using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO Tbl_Roller (RolID, RolAdi) VALUES (@id, @rol)", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", i + 1);
                            cmd.Parameters.AddWithValue("@rol", roller[i]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 6. Firmalar (50+ firma)
                    string[] firmaUnvanlari = {
                        "Anadolu Enerji A.Ş.", "Marmara Gıda San. Ltd.", "Ege Kimya A.Ş.", "Bursa Tekstil Ltd.",
                        "Ankara İlaç A.Ş.", "Konya Ambalaj San.", "Antalya Organik Gıda", "Adana Enerji Ltd.",
                        "Gaziantep Gıda A.Ş.", "Kocaeli Kimya San.", "Denizli Tekstil A.Ş.", "Eskişehir Ambalaj",
                        "Sakarya Enerji Ltd.", "Aydın Organik Tarım", "Balıkesir Gıda A.Ş.", "Manisa İlaç San.",
                        "İstanbul Kimya Ltd.", "Ankara Ambalaj A.Ş.", "İzmir Enerji San.", "Bursa Tekstil A.Ş.",
                        "Trabzon Balıkçılık A.Ş.", "Samsun Tarım Ltd.", "Kayseri Sanayi A.Ş.", "Erzurum Enerji San.",
                        "Malatya Gıda Ltd.", "Kırıkkale Kimya A.Ş.", "Zonguldak Maden San.", "Sivas Tekstil Ltd.",
                        "Kastamonu Orman Ürünleri", "Rize Çay San. A.Ş.", "Ordu Fındık İşleme", "Giresun Ceviz A.Ş.",
                        "Tokat Tarım Koop.", "Amasya Organik Ürünler", "Çorum Unlu Mamuller", "Yozgat Hayvancılık A.Ş.",
                        "Nevşehir Turizm Ltd.", "Kırşehir Gıda San.", "Aksaray Tekstil A.Ş.", "Niğde Maden Ltd.",
                        "Karaman Tarım A.Ş.", "Konya Şeker San.", "Afyon Mermer Ltd.", "Uşak Seramik A.Ş.",
                        "Kütahya Porselen San.", "Bilecik Kağıt Ltd.", "Eskişehir Makine A.Ş.", "Bursa Otomotiv San.",
                        "İzmir Teknoloji Ltd.", "Ankara Yazılım A.Ş.", "İstanbul Finans Ltd.", "Kocaeli Lojistik A.Ş."
                    };

                    string[] firmaSektorleri = { "Enerji", "Gıda", "Kimya", "Tekstil", "İlaç", "Ambalaj", "Tarım", "Sanayi" };
                    string[] firmaAdresleri = {
                        "Organize Sanayi Bölgesi 1. Cadde No:12", "Sanayi Sitesi B Blok No:45",
                        "OSB 2. Cadde No:78", "İş Merkezi Kat:3 Daire:15", "Sanayi Mahallesi Atatürk Cad. No:23",
                        "Ticaret Merkezi A Blok", "Endüstri Bölgesi 5. Sokak", "Teknoloji Parkı Bina:2"
                    };

                    // Firma ID'lerini saklamak için liste
                    List<int> firmaIDListesi = new List<int>();

                    for (int i = 0; i < firmaUnvanlari.Length; i++)
                    {
                        int sektorID = rnd.Next(1, sektorler.Length + 1);
                        int sehirID = rnd.Next(1, sehirler.Length + 1);
                        int durumID = rnd.Next(1, 4);
                        string vergiNo = (1000000000 + i).ToString();
                        string adres = firmaAdresleri[rnd.Next(firmaAdresleri.Length)];
                        string logoUrl = $"firmalar/firma_{i + 1:D3}.png";

                        try
                        {
                            // Önce firmanın var olup olmadığını kontrol et
                            using (var checkCmd = new SQLiteCommand(
                                "SELECT FirmaID FROM Tbl_Firmalar WHERE Unvan = @unvan OR VergiNo = @vergi", conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@unvan", firmaUnvanlari[i]);
                                checkCmd.Parameters.AddWithValue("@vergi", vergiNo);
                                object existingID = checkCmd.ExecuteScalar();

                                if (existingID != null)
                                {
                                    // Firma zaten varsa, mevcut ID'yi kullan
                                    firmaIDListesi.Add(Convert.ToInt32(existingID));
                                }
                                else
                                {
                                    // Yeni firma ekle ve ID'yi al
                                    using (var insertCmd = new SQLiteCommand(
                                        "INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, LogoUrl, DurumID) VALUES (@unvan, @vergi, @sektor, @sehir, @adres, @logo, @durum); SELECT last_insert_rowid();", conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@unvan", firmaUnvanlari[i]);
                                        insertCmd.Parameters.AddWithValue("@vergi", vergiNo);
                                        insertCmd.Parameters.AddWithValue("@sektor", sektorID);
                                        insertCmd.Parameters.AddWithValue("@sehir", sehirID);
                                        insertCmd.Parameters.AddWithValue("@adres", adres);
                                        insertCmd.Parameters.AddWithValue("@logo", logoUrl);
                                        insertCmd.Parameters.AddWithValue("@durum", durumID);
                                        
                                        object newID = insertCmd.ExecuteScalar();
                                        if (newID != null)
                                        {
                                            firmaIDListesi.Add(Convert.ToInt32(newID));
                                            eklenenSayisi++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Firma eklenirken hata: {firmaUnvanlari[i]} - {ex.Message}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Firmalar eklendi: {eklenenSayisi}/{firmaUnvanlari.Length}, Toplam ID: {firmaIDListesi.Count}");
                    eklenenSayisi = 0;

                    // 7. Çiftlikler (50+ çiftlik)
                    string[] ciftlikUnvanlari = {
                        "Bereketli Topraklar Çiftliği", "Yeşil Vadi Organik Çiftlik", "Altın Başak Tarım",
                        "Doğal Hayat Çiftliği", "Verimli Topraklar", "Organik Bahçe Çiftliği", "Ege Organik Tarım",
                        "Çukurova Tarım A.Ş.", "Gaziantep Organik", "Kocaeli Tarım Ltd.", "Denizli Organik Çiftlik",
                        "Eskişehir Tarım A.Ş.", "Sakarya Organik Tarım", "Aydın Organik Çiftlik", "Balıkesir Tarım Ltd.",
                        "Manisa Organik Bahçe", "İstanbul Organik Tarım", "Ankara Organik Çiftlik", "İzmir Organik Tarım",
                        "Bursa Organik Çiftlik", "Trabzon Çay Bahçesi", "Rize Organik Tarım", "Ordu Fındık Bahçesi",
                        "Giresun Ceviz Çiftliği", "Tokat Tarım Koop.", "Amasya Organik Ürünler", "Çorum Hububat Çiftliği",
                        "Yozgat Hayvancılık", "Nevşehir Bağcılık", "Kırşehir Tarım A.Ş.", "Aksaray Organik Çiftlik",
                        "Niğde Tarım Ltd.", "Karaman Organik Ürünler", "Konya Hububat Çiftliği", "Afyon Tarım A.Ş.",
                        "Uşak Organik Bahçe", "Kütahya Tarım Koop.", "Bilecik Organik Çiftlik", "Eskişehir Tarım Ltd.",
                        "Bursa Organik Tarım", "İzmir Zeytin Bahçesi", "Aydın İncir Çiftliği", "Muğla Zeytin A.Ş.",
                        "Antalya Seracılık", "Mersin Narenciye Bahçesi", "Adana Pamuk Çiftliği", "Gaziantep Fıstık Bahçesi",
                        "Şanlıurfa Tarım A.Ş.", "Diyarbakır Organik Çiftlik", "Mardin Tarım Ltd.", "Batman Organik Ürünler"
                    };

                    string[] ciftlikAdresleri = {
                        "Merkez Köyü", "Tarım Alanı", "Ovası", "Bölgesi", "Mahallesi", "İlçesi",
                        "Köyü", "Mezrası", "Beldesi", "Kasabası", "Nahiyesi"
                    };

                    // Çiftlik ID'lerini saklamak için liste
                    List<int> ciftlikIDListesi = new List<int>();

                    for (int i = 0; i < ciftlikUnvanlari.Length; i++)
                    {
                        int sektorID = 1; // Tarım sektörü
                        int sehirID = rnd.Next(1, sehirler.Length + 1);
                        int durumID = rnd.Next(1, 4);
                        string vergiNo = (2000000000 + i).ToString();
                        string sehirAdi = sehirler[sehirID - 1];
                        string adres = $"{sehirAdi} {ciftlikAdresleri[rnd.Next(ciftlikAdresleri.Length)]}";
                        string logoUrl = $"ciftlikler/ciftlik_{i + 1:D3}.png";

                        try
                        {
                            // Önce çiftliğin var olup olmadığını kontrol et
                            using (var checkCmd = new SQLiteCommand(
                                "SELECT CiftlikID FROM Tbl_Ciftlikler WHERE Unvan = @unvan OR VergiNo = @vergi", conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@unvan", ciftlikUnvanlari[i]);
                                checkCmd.Parameters.AddWithValue("@vergi", vergiNo);
                                object existingID = checkCmd.ExecuteScalar();

                                if (existingID != null)
                                {
                                    // Çiftlik zaten varsa, mevcut ID'yi kullan
                                    ciftlikIDListesi.Add(Convert.ToInt32(existingID));
                                }
                                else
                                {
                                    // Yeni çiftlik ekle ve ID'yi al
                                    using (var insertCmd = new SQLiteCommand(
                                        "INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, LogoUrl, DurumID) VALUES (@unvan, @vergi, @sektor, @sehir, @adres, @logo, @durum); SELECT last_insert_rowid();", conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@unvan", ciftlikUnvanlari[i]);
                                        insertCmd.Parameters.AddWithValue("@vergi", vergiNo);
                                        insertCmd.Parameters.AddWithValue("@sektor", sektorID);
                                        insertCmd.Parameters.AddWithValue("@sehir", sehirID);
                                        insertCmd.Parameters.AddWithValue("@adres", adres);
                                        insertCmd.Parameters.AddWithValue("@logo", logoUrl);
                                        insertCmd.Parameters.AddWithValue("@durum", durumID);
                                        
                                        object newID = insertCmd.ExecuteScalar();
                                        if (newID != null)
                                        {
                                            ciftlikIDListesi.Add(Convert.ToInt32(newID));
                                            eklenenSayisi++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Çiftlik eklenirken hata: {ciftlikUnvanlari[i]} - {ex.Message}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Çiftlikler eklendi: {eklenenSayisi}/{ciftlikUnvanlari.Length}, Toplam ID: {ciftlikIDListesi.Count}");
                    eklenenSayisi = 0;

                    // 8. Kullanıcılar (50+ kullanıcı - firmalar, çiftlikler ve adminler)
                    // Admin kullanıcıları
                    try
                    {
                        using (var cmd = new SQLiteCommand(
                            "INSERT OR IGNORE INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (3, 'sanayi_admin', '123456', NULL, 2)", conn, transaction))
                        {
                            if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Admin kullanıcı eklenirken hata: {ex.Message}");
                    }
                    try
                    {
                        using (var cmd = new SQLiteCommand(
                            "INSERT OR IGNORE INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (4, 'ziraat_admin', '123456', NULL, 2)", conn, transaction))
                        {
                            if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Admin kullanıcı eklenirken hata: {ex.Message}");
                    }

                    // Firma kullanıcıları - gerçek firma ID'lerini kullan
                    int firmaKullaniciIndex = 1;
                    foreach (int firmaID in firmaIDListesi)
                    {
                        int durum = rnd.Next(1, 4);
                        try
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT OR IGNORE INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (1, @kullanici, '123456', @ilgili, @durum)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@kullanici", $"firma{firmaKullaniciIndex}");
                                cmd.Parameters.AddWithValue("@ilgili", firmaID);
                                cmd.Parameters.AddWithValue("@durum", durum);
                                if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Firma kullanıcı eklenirken hata: firma{firmaKullaniciIndex} (ID: {firmaID}) - {ex.Message}");
                        }
                        firmaKullaniciIndex++;
                    }

                    // Çiftlik kullanıcıları - gerçek çiftlik ID'lerini kullan
                    int kullaniciIndex = 1;
                    foreach (int ciftlikID in ciftlikIDListesi)
                    {
                        int durum = rnd.Next(1, 4);
                        try
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT OR IGNORE INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (2, @kullanici, '123456', @ilgili, @durum)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@kullanici", $"ciftlik{kullaniciIndex}");
                                cmd.Parameters.AddWithValue("@ilgili", ciftlikID);
                                cmd.Parameters.AddWithValue("@durum", durum);
                                if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Çiftlik kullanıcı eklenirken hata: ciftlik{kullaniciIndex} (ID: {ciftlikID}) - {ex.Message}");
                        }
                        kullaniciIndex++;
                    }

                    // 9. Çiftlik Ürünleri (50+ ürün)
                    string[] urunAdlari = {
                        "Organik Kompost", "Tahıl Samanı", "Sebze Atığı", "Meyve Posası", "Zeytin Karasuyu",
                        "Bağ Atığı", "Süt Atığı", "Hayvan Gübresi", "Yem Artığı", "Bitki Sapı",
                        "Mısır Koçanı", "Ayçiçeği Sapı", "Pamuk Atığı", "Şeker Pancarı Yaprağı", "Domates Sapı",
                        "Pirinç Kabuğu", "Buğday Samanı", "Arpa Samanı", "Yulaf Samanı", "Çavdar Samanı",
                        "Mısır Sapı", "Soya Atığı", "Kanola Atığı", "Pamuk Çiğidi", "Zeytin Prinası",
                        "Fındık Kabuğu", "Ceviz Kabuğu", "Badem Kabuğu", "Çay Atığı", "Kahve Atığı",
                        "Şeker Kamışı Atığı", "Bambu Atığı", "Yosun", "Alg", "Organik Gübre",
                        "Kompost Karışımı", "Tavuk Gübresi", "Sığır Gübresi", "Koyun Gübresi", "Keçi Gübresi",
                        "At Gübresi", "Tavşan Gübresi", "Organik Toprak", "Humus", "Vermikompost",
                        "Biyogaz Atığı", "Fermente Atık", "Organik Pelet", "Biyokütle Briket", "Organik Toz"
                    };

                    // Ürün ID'lerini saklamak için liste
                    List<int> urunIDListesi = new List<int>();
                    
                    foreach (int ciftlikID in ciftlikIDListesi)
                    {
                        if (urunIDListesi.Count >= 50) break;
                        
                        int urunSayisi = rnd.Next(1, 4); // Her çiftliğe 1-3 ürün
                        for (int j = 0; j < urunSayisi && urunIDListesi.Count < 50; j++)
                        {
                            int kategoriID = rnd.Next(1, kategoriler.Length + 1);
                            string urunAdi = urunAdlari[rnd.Next(urunAdlari.Length)];
                            double miktar = Math.Round(rnd.NextDouble() * 450 + 50, 2); // 50-500 ton
                            int durum = rnd.Next(1, 4);

                            try
                            {
                                using (var cmd = new SQLiteCommand(
                                    "INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (@ciftlik, @kategori, @urun, @miktar, @durum); SELECT last_insert_rowid();", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ciftlik", ciftlikID);
                                    cmd.Parameters.AddWithValue("@kategori", kategoriID);
                                    cmd.Parameters.AddWithValue("@urun", urunAdi);
                                    cmd.Parameters.AddWithValue("@miktar", miktar);
                                    cmd.Parameters.AddWithValue("@durum", durum);
                                    
                                    object newID = cmd.ExecuteScalar();
                                    if (newID != null)
                                    {
                                        urunIDListesi.Add(Convert.ToInt32(newID));
                                        eklenenSayisi++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ürün eklenirken hata: {urunAdi} - {ex.Message}");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Ürünler eklendi: {eklenenSayisi} ürün, Toplam ID: {urunIDListesi.Count}");
                    eklenenSayisi = 0;

                    // 10. Alım Talepleri (50+ talep)
                    string[] talepNotlari = {
                        "Organik atık talebi - kompost üretimi için",
                        "Biyokütle enerji üretimi için saman talebi",
                        "Hayvan yemi üretimi için atık talebi",
                        "Gübre üretimi için organik atık",
                        "Biyogaz üretimi için tarımsal atık",
                        "Yakıt üretimi için biyokütle talebi",
                        "Organik gübre üretimi için atık",
                        "Enerji üretimi için biyokütle",
                        "Kompost üretimi için organik atık",
                        "Hayvan yemi için tahıl samanı",
                        "Biyogaz tesisi için organik atık",
                        "Gübre fabrikası için tarımsal atık"
                    };

                    string[] reddetmeNedenleri = {
                        "Miktar talebi çok yüksek",
                        "Kalite standartlarına uygun değil",
                        "Teslimat tarihi uygun değil",
                        "Fiyat anlaşması sağlanamadı",
                        "Belgeler eksik",
                        "Kapasite yetersiz",
                        "Nakliye maliyeti yüksek"
                    };

                    for (int i = 1; i <= 50; i++)
                    {
                        // Gerçek ID'lerden rastgele seç
                        int firmaID = firmaIDListesi.Count > 0 ? firmaIDListesi[rnd.Next(firmaIDListesi.Count)] : 1;
                        int ciftlikID = ciftlikIDListesi.Count > 0 ? ciftlikIDListesi[rnd.Next(ciftlikIDListesi.Count)] : 1;
                        int urunID = urunIDListesi.Count > 0 ? urunIDListesi[rnd.Next(urunIDListesi.Count)] : 1;
                        double miktar = Math.Round(rnd.NextDouble() * 250 + 50, 2); // 50-300 ton
                        int durum = rnd.Next(1, 4);
                        string notlar = talepNotlari[rnd.Next(talepNotlari.Length)];
                        string reddetmeNedeni = durum == 3 ? reddetmeNedenleri[rnd.Next(reddetmeNedenleri.Length)] : "";
                        DateTime talepTarihi = DateTime.Now.AddDays(-rnd.Next(1, 180));

                        try
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, ReddetmeNedeni, TalepTarihi) VALUES (@firma, @ciftlik, @urun, @miktar, @not, @durum, @red, @tarih)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@firma", firmaID);
                                cmd.Parameters.AddWithValue("@ciftlik", ciftlikID);
                                cmd.Parameters.AddWithValue("@urun", urunID);
                                cmd.Parameters.AddWithValue("@miktar", miktar);
                                cmd.Parameters.AddWithValue("@not", notlar);
                                cmd.Parameters.AddWithValue("@durum", durum);
                                cmd.Parameters.AddWithValue("@red", reddetmeNedeni);
                                cmd.Parameters.AddWithValue("@tarih", talepTarihi.ToString("yyyy-MM-dd HH:mm:ss"));
                                if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Talep eklenirken hata: {ex.Message}");
                        }
                    }

                    // 11. Çiftlik Belgeleri (50+ belge)
                    string[] ciftlikBelgeleri = {
                        "Çiftlik Ruhsatı", "Organik Tarım Sertifikası", "Gıda Güvenliği Belgesi",
                        "Hayvan Sağlığı Raporu", "Toprak Analiz Raporu", "Su Analiz Raporu",
                        "Çevre İzin Belgesi", "İşletme Belgesi", "Vergi Levhası",
                        "İmza Sirküleri", "Oda Kayıt Belgesi", "ISO Sertifikası"
                    };

                    // Çiftlik belgeleri - gerçek çiftlik ID'lerini kullan
                    // Her çiftliğe en az 1 belge ekle
                    foreach (int ciftlikID in ciftlikIDListesi)
                    {
                        int belgeSayisi = rnd.Next(1, 4); // Her çiftliğe 1-3 belge
                        for (int j = 0; j < belgeSayisi; j++)
                        {
                            string belgeAdi = ciftlikBelgeleri[rnd.Next(ciftlikBelgeleri.Length)];
                            // Belgeler klasöründen rastgele bir dosya seç
                            string rastgeleBelge = mevcutBelgeler[rnd.Next(mevcutBelgeler.Length)];
                            string dosyaYolu = $"Belgeler/{rastgeleBelge}";

                            try
                            {
                                using (var cmd = new SQLiteCommand(
                                    "INSERT INTO Tbl_CiftlikBelgeleri (CiftlikID, BelgeAdi, DosyaYolu) VALUES (@ciftlik, @belge, @dosya)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ciftlik", ciftlikID);
                                    cmd.Parameters.AddWithValue("@belge", belgeAdi);
                                    cmd.Parameters.AddWithValue("@dosya", dosyaYolu);
                                    if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Belge eklenirken hata (CiftlikID: {ciftlikID}): {ex.Message}");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Çiftlik belgeleri eklendi: {eklenenSayisi} belge, {ciftlikIDListesi.Count} çiftlik için");
                    eklenenSayisi = 0;

                    // 12. Firma Belgeleri (50+ belge)
                    string[] firmaBelgeleri = {
                        "Vergi Levhası", "İmza Sirküleri", "Oda Kayıt Belgesi", "ISO Sertifikası",
                        "Çevre İzin Belgesi", "İşletme Ruhsatı", "SGK Belgesi", "Ticaret Sicil Gazetesi",
                        "Faaliyet Belgesi", "İhracat Belgesi", "Kalite Belgesi", "Güvenlik Belgesi"
                    };

                    // Firma belgeleri - gerçek firma ID'lerini kullan
                    // Her firmaya en az 1 belge ekle
                    foreach (int firmaID in firmaIDListesi)
                    {
                        int belgeSayisi = rnd.Next(1, 4); // Her firmaya 1-3 belge
                        for (int j = 0; j < belgeSayisi; j++)
                        {
                            string belgeAdi = firmaBelgeleri[rnd.Next(firmaBelgeleri.Length)];
                            // Belgeler klasöründen rastgele bir dosya seç
                            string rastgeleBelge = mevcutBelgeler[rnd.Next(mevcutBelgeler.Length)];
                            string dosyaYolu = $"Belgeler/{rastgeleBelge}";

                            try
                            {
                                using (var cmd = new SQLiteCommand(
                                    "INSERT INTO Tbl_FirmaBelgeleri (FirmaID, BelgeAdi, DosyaYolu) VALUES (@firma, @belge, @dosya)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@firma", firmaID);
                                    cmd.Parameters.AddWithValue("@belge", belgeAdi);
                                    cmd.Parameters.AddWithValue("@dosya", dosyaYolu);
                                    if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Belge eklenirken hata (FirmaID: {firmaID}): {ex.Message}");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Firma belgeleri eklendi: {eklenenSayisi} belge, {firmaIDListesi.Count} firma için");
                    eklenenSayisi = 0;

                    // 13. Ürün Belgeleri (50+ belge)
                    string[] urunBelgeleri = {
                        "Analiz Raporu", "Kalite Belgesi", "Organik Sertifika", "Güvenlik Raporu",
                        "Test Sonuçları", "Üretim Belgesi", "İhracat Belgesi", "Gıda Güvenliği Belgesi",
                        "Toprak Analizi", "Su Analizi", "Hav Analizi", "Mikrobiyolojik Analiz"
                    };

                    // Ürün belgeleri - gerçek ürün ID'lerini kullan
                    // Her ürüne en az 1 belge ekle
                    foreach (int urunID in urunIDListesi)
                    {
                        int belgeSayisi = rnd.Next(1, 3); // Her ürüne 1-2 belge
                        for (int j = 0; j < belgeSayisi; j++)
                        {
                            string belgeAdi = urunBelgeleri[rnd.Next(urunBelgeleri.Length)];
                            // Belgeler klasöründen rastgele bir dosya seç
                            string rastgeleBelge = mevcutBelgeler[rnd.Next(mevcutBelgeler.Length)];
                            string dosyaYolu = $"Belgeler/{rastgeleBelge}";

                            try
                            {
                                using (var cmd = new SQLiteCommand(
                                    "INSERT INTO Tbl_UrunBelgeleri (UrunID, BelgeAdi, DosyaYolu) VALUES (@urun, @belge, @dosya)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@urun", urunID);
                                    cmd.Parameters.AddWithValue("@belge", belgeAdi);
                                    cmd.Parameters.AddWithValue("@dosya", dosyaYolu);
                                    if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Belge eklenirken hata (UrunID: {urunID}): {ex.Message}");
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Ürün belgeleri eklendi: {eklenenSayisi} belge, {urunIDListesi.Count} ürün için");
                    eklenenSayisi = 0;

                    // 14. İşlem Logları (50+ log)
                    string[] logMesajlari = {
                        "Sanayi admin giriş yaptı", "Ziraat admin giriş yaptı", "Firma kullanıcısı giriş yaptı",
                        "Çiftlik kullanıcısı giriş yaptı", "Yeni firma kaydı onaylandı", "Yeni çiftlik kaydı onaylandı",
                        "Firma kaydı reddedildi", "Çiftlik kaydı reddedildi", "Ürün talebi oluşturuldu",
                        "Ürün talebi onaylandı", "Ürün talebi reddedildi", "Genel rapor oluşturuldu",
                        "SDG raporu oluşturuldu", "Excel raporu dışa aktarıldı", "Kullanıcı çıkış yaptı",
                        "Firma bilgileri güncellendi", "Çiftlik bilgileri güncellendi", "Ürün bilgileri güncellendi",
                        "Belge yüklendi", "Belge silindi", "Talep durumu değiştirildi",
                        "Kullanıcı şifresi değiştirildi", "Rapor görüntülendi", "Veri dışa aktarıldı"
                    };

                    for (int i = 1; i <= 50; i++)
                    {
                        int rolID = rnd.Next(1, 5);
                        string mesaj = logMesajlari[rnd.Next(logMesajlari.Length)];
                        DateTime logTarihi = DateTime.Now.AddDays(-rnd.Next(1, 90)).AddHours(-rnd.Next(0, 24)).AddMinutes(-rnd.Next(0, 60));

                        try
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT INTO Tbl_IslemLoglari (Aciklama, RolID, IslemTarihi) VALUES (@mesaj, @rol, @tarih)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@mesaj", mesaj);
                                cmd.Parameters.AddWithValue("@rol", rolID);
                                cmd.Parameters.AddWithValue("@tarih", logTarihi.ToString("yyyy-MM-dd HH:mm:ss"));
                                if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Log eklenirken hata: {ex.Message}");
                        }
                    }

                    // 15. SDG Rapor Verisi (50+ kayıt)
                    for (int i = 1; i <= 50; i++)
                    {
                        int talepID = rnd.Next(1, 51);
                        double atikTon = Math.Round(rnd.NextDouble() * 400 + 100, 2); // 100-500 ton
                        double co2Ton = Math.Round(atikTon * 0.5, 2); // CO2 hesaplama
                        double ekonomikDeger = Math.Round(atikTon * 1000, 2); // Ekonomik değer
                        DateTime raporTarihi = DateTime.Now.AddDays(-rnd.Next(1, 120)).AddHours(-rnd.Next(0, 24));

                        try
                        {
                            using (var cmd = new SQLiteCommand(
                                "INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL, IslemTarihi) VALUES (@talep, @atik, @co2, @deger, @tarih)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@talep", talepID);
                                cmd.Parameters.AddWithValue("@atik", atikTon);
                                cmd.Parameters.AddWithValue("@co2", co2Ton);
                                cmd.Parameters.AddWithValue("@deger", ekonomikDeger);
                                cmd.Parameters.AddWithValue("@tarih", raporTarihi.ToString("yyyy-MM-dd HH:mm:ss"));
                                if (cmd.ExecuteNonQuery() > 0) eklenenSayisi++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"SDG rapor verisi eklenirken hata: {ex.Message}");
                        }
                    }

                            // Transaction'ı commit et
                            transaction.Commit();

                            MessageBox.Show("Sentetik veriler başarıyla eklendi! Her tabloda minimum 50 veri bulunmaktadır.",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception innerEx)
                        {
                            // Hata durumunda rollback
                            transaction.Rollback();
                            throw innerEx;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sentetik veri eklenirken hata oluştu: {ex.Message}",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tüm tabloları oluşturur
        /// </summary>
        private static void CreateTables()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string createTablesSQL = @"
                -- Sabit (Lookup) Tablolar
                CREATE TABLE IF NOT EXISTS Tbl_Sehirler (
                    SehirID INTEGER PRIMARY KEY AUTOINCREMENT,
                    SehirAdi TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_Sektorler (
                    SektorID INTEGER PRIMARY KEY AUTOINCREMENT,
                    SektorAdi TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_UrunKategorileri (
                    KategoriID INTEGER PRIMARY KEY AUTOINCREMENT,
                    KategoriAdi TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_OnayDurumlari (
                    DurumID INTEGER PRIMARY KEY AUTOINCREMENT,
                    DurumAdi TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_Roller (
                    RolID INTEGER PRIMARY KEY AUTOINCREMENT,
                    RolAdi TEXT
                );

                -- Ana Aktör Tabloları
                CREATE TABLE IF NOT EXISTS Tbl_Firmalar (
                    FirmaID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Unvan TEXT,
                    VergiNo TEXT,
                    SektorID INTEGER,
                    SehirID INTEGER,
                    Adres TEXT,
                    LogoUrl TEXT,
                    DurumID INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Tbl_Ciftlikler (
                    CiftlikID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Unvan TEXT,
                    VergiNo TEXT,
                    SektorID INTEGER,
                    SehirID INTEGER,
                    Adres TEXT,
                    LogoUrl TEXT,
                    DurumID INTEGER DEFAULT 1
                );

                -- Kullanıcı Giriş Tablosu
                CREATE TABLE IF NOT EXISTS Tbl_Kullanicilar (
                    KullaniciID INTEGER PRIMARY KEY AUTOINCREMENT,
                    RolID INTEGER,
                    KullaniciAdi TEXT,
                    SifreHash TEXT,
                    IlgiliID INTEGER,
                    DurumID INTEGER,
                    KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                -- Ürün ve Talep Yönetimi
                CREATE TABLE IF NOT EXISTS Tbl_CiftlikUrunleri (
                    UrunID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CiftlikID INTEGER,
                    UrunKategoriID INTEGER,
                    UrunAdi TEXT,
                    MiktarTon REAL,
                    DurumID INTEGER
                );

                CREATE TABLE IF NOT EXISTS Tbl_AlimTalepleri (
                    TalepID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirmaID INTEGER,
                    HedefCiftlikID INTEGER,
                    UrunID INTEGER,
                    TalepMiktarTon REAL,
                    FirmaNotu TEXT,
                    DurumID INTEGER,
                    ReddetmeNedeni TEXT,
                    TalepTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                -- Belge Yönetimi
                CREATE TABLE IF NOT EXISTS Tbl_CiftlikBelgeleri (
                    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CiftlikID INTEGER,
                    BelgeAdi TEXT,
                    DosyaYolu TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_FirmaBelgeleri (
                    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirmaID INTEGER,
                    BelgeAdi TEXT,
                    DosyaYolu TEXT
                );

                CREATE TABLE IF NOT EXISTS Tbl_UrunBelgeleri (
                    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
                    UrunID INTEGER,
                    BelgeAdi TEXT,
                    DosyaYolu TEXT
                );

                -- Raporlama ve Loglar
                CREATE TABLE IF NOT EXISTS Tbl_IslemLoglari (
                    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Aciklama TEXT,
                    RolID INTEGER,
                    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Tbl_SdgRaporVerisi (
                    RaporVeriID INTEGER PRIMARY KEY AUTOINCREMENT,
                    OnaylananTalepID INTEGER,
                    GeriKazanilanAtikTon REAL,
                    EngellenenCO2Ton REAL,
                    EkonomikDegerTL REAL,
                    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                ";

                using (var cmd = new SQLiteCommand(createTablesSQL, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// SELECT sorgusu çalıştırır ve DataTable döndürür
        /// </summary>
        public static DataTable ExecuteQuery(string query, params SQLiteParameter[] parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        using (var adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sorgu hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        /// <summary>
        /// INSERT, UPDATE, DELETE sorguları için
        /// </summary>
        public static int ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        /// <summary>
        /// Tek bir değer döndüren sorgular için (COUNT, MAX vb.)
        /// </summary>
        public static object ExecuteScalar(string query, params SQLiteParameter[] parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sorgu hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// İşlem loglarına kayıt ekler
        /// </summary>
        public static void LogIslem(string aciklama)
        {
            int rolID = Session.RolID > 0 ? Session.RolID : 0;
            string query = "INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES (@aciklama, @rolID)";
            ExecuteNonQuery(query,
                new SQLiteParameter("@aciklama", aciklama),
                new SQLiteParameter("@rolID", rolID));
        }

        /// <summary>
        /// İşlem loglarına kayıt ekler (RolID ile)
        /// </summary>
        public static void LogIslem(string aciklama, int rolID)
        {
            string query = "INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES (@aciklama, @rolID)";
            ExecuteNonQuery(query,
                new SQLiteParameter("@aciklama", aciklama),
                new SQLiteParameter("@rolID", rolID));
        }
    }
}
