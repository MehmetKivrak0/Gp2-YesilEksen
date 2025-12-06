using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
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
        
        // Connection string
        private static string ConnectionString => $"Data Source={DbPath};Version=3;";

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
                    InsertTestData();
                    MessageBox.Show("Veritabanı oluşturuldu ve test verileri eklendi!", 
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Veritabanı var, tabloları kontrol et
                    CreateTables();
                    
                    // Ürün tablosu boşsa verileri ekle
                    if (IsTableEmpty("Tbl_CiftlikUrunleri"))
                    {
                        InsertTestData();
                        MessageBox.Show("Test verileri eklendi!", 
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı başlatılırken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tablonun boş olup olmadığını kontrol eder
        /// </summary>
        private static bool IsTableEmpty(string tableName)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        return result == null || Convert.ToInt32(result) == 0;
                    }
                }
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Veritabanını zorla sıfırlar (test amaçlı)
        /// </summary>
        public static void ResetDatabase()
        {
            try
            {
                if (File.Exists(DbPath))
                {
                    File.Delete(DbPath);
                }
                SQLiteConnection.CreateFile(DbPath);
                CreateTables();
                InsertTestData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı sıfırlanırken hata: {ex.Message}", 
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
                    RolID INTEGER DEFAULT 0,
                    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Tbl_SdgRaporVerisi (
                    RaporVeriID INTEGER PRIMARY KEY AUTOINCREMENT,
                    OnaylananTalepID INTEGER,
                    GeriKazanilanAtikTon REAL,
                    EngellenenCO2Ton REAL,
                    EkonomikDegerTL REAL,
                    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

                using (var cmd = new SQLiteCommand(createTablesSQL, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Test verilerini ekler
        /// </summary>
        private static void InsertTestData()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                
                string insertSQL = @"
                -- Şehirler (15 il)
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Manisa');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('İstanbul');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Ankara');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('İzmir');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Bursa');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Konya');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Antalya');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Adana');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Gaziantep');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Kocaeli');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Denizli');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Eskişehir');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Sakarya');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Aydın');
                INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Balıkesir');

                -- Sektörler (8 sektör)
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Tarım');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Sanayi');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('İlaç');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Enerji');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Gıda');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Tekstil');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Kimya');
                INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Ambalaj');

                -- Ürün Kategorileri
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Organik Atık');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Plastik Atık');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Metal Atık');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Cam Atık');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Kağıt Atık');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Biyokütle');
                INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Tarımsal Atık');

                -- Onay Durumları
                INSERT INTO Tbl_OnayDurumlari (DurumAdi) VALUES ('Onay Bekliyor');
                INSERT INTO Tbl_OnayDurumlari (DurumAdi) VALUES ('Onaylandı');
                INSERT INTO Tbl_OnayDurumlari (DurumAdi) VALUES ('Reddedildi');

                -- Roller
                INSERT INTO Tbl_Roller (RolAdi) VALUES ('Firma');
                INSERT INTO Tbl_Roller (RolAdi) VALUES ('Ciftlik');
                INSERT INTO Tbl_Roller (RolAdi) VALUES ('Sanayi Odası Admin');
                INSERT INTO Tbl_Roller (RolAdi) VALUES ('Ziraat Odası Admin');

                -- Admin Kullanıcıları (şifre: 123456)
                INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (3, 'sanayi_admin', '123456', NULL, 2);
                INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) VALUES (4, 'ziraat_admin', '123456', NULL, 2);

                -- ===================== 50 FİRMA =====================
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Anadolu Enerji A.Ş.', '1010101010', 4, 1, 'Manisa OSB 1. Cadde No:12', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Marmara Gıda San. Ltd.', '1020203030', 5, 2, 'Hadımköy Sanayi Sitesi B Blok', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Başkent İlaç A.Ş.', '1030304040', 3, 3, 'Ankara Teknokent Binası K:5', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Ege Kimya San. A.Ş.', '1040405050', 7, 4, 'Atatürk OSB No:45', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Nilüfer Tekstil Ltd.', '1050506060', 6, 5, 'DOSAB 3. Cadde No:18', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Konya Sanayi A.Ş.', '1060607070', 2, 6, 'Konya OSB 2. Bölge No:33', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akdeniz Ambalaj Ltd.', '1070708080', 8, 7, 'Antalya Serbest Bölge No:7', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çukurova Gıda A.Ş.', '1080809090', 5, 8, 'Adana Hacı Sabancı OSB No:21', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('GAP Enerji Ltd.', '1090910101', 4, 9, 'Gaziantep OSB 5. Cadde No:14', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Gebze Kimya A.Ş.', '1101011111', 7, 10, 'Gebze OSB 4. Bölge No:28', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Pamukkale Tekstil Ltd.', '1111112121', 6, 11, 'Denizli OSB 1. Cadde No:9', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Porsuk İlaç A.Ş.', '1121213131', 3, 12, 'Eskişehir Teknokent No:3', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Sakarya Gıda Ltd.', '1131314141', 5, 13, 'Sakarya ASO 2. Bölge No:16', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Menderes Kimya A.Ş.', '1141415151', 7, 14, 'Aydın OSB No:42', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Bandırma Enerji Ltd.', '1151516161', 4, 15, 'Balıkesir OSB 3. Cadde No:25', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Trakya Ambalaj A.Ş.', '1161617171', 8, 2, 'Çerkezköy OSB No:31', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Karadeniz Gıda Ltd.', '1171718181', 5, 3, 'Ankara Sincan OSB No:19', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Batı Anadolu Tekstil A.Ş.', '1181819191', 6, 4, 'İzmir Kemalpaşa OSB No:8', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Osmangazi Sanayi Ltd.', '1191920202', 2, 5, 'Bursa Nilüfer OSB No:44', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('İç Anadolu Enerji A.Ş.', '1202021212', 4, 6, 'Konya 2. OSB No:37', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akdeniz Kimya Ltd.', '1212122222', 7, 7, 'Antalya OSB 2. Cadde No:11', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çukurova İlaç A.Ş.', '1222223232', 3, 8, 'Adana Ceyhan OSB No:6', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Güneydoğu Tekstil Ltd.', '1232324242', 6, 9, 'Gaziantep 4. OSB No:29', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Kocaeli Ambalaj A.Ş.', '1242425252', 8, 10, 'Dilovası OSB No:52', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Denizli Gıda Ltd.', '1252526262', 5, 11, 'Denizli 2. OSB No:17', 1);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Eskişehir Sanayi A.Ş.', '1262627272', 2, 12, 'Eskişehir OSB 3. Bölge No:23', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Adapazarı Kimya Ltd.', '1272728282', 7, 13, 'Sakarya 2. OSB No:38', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Söke Tekstil A.Ş.', '1282829292', 6, 14, 'Aydın Söke OSB No:13', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Gönen Enerji Ltd.', '1292930303', 4, 15, 'Balıkesir Gönen OSB No:4', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Salihli Gıda A.Ş.', '1303031313', 5, 1, 'Manisa Salihli OSB No:27', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Silivri Ambalaj Ltd.', '1313132323', 8, 2, 'İstanbul Silivri OSB No:35', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Polatlı Sanayi A.Ş.', '1323233333', 2, 3, 'Ankara Polatlı OSB No:41', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Torbalı Kimya Ltd.', '1333334343', 7, 4, 'İzmir Torbalı OSB No:22', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('İnegöl Tekstil A.Ş.', '1343435353', 6, 5, 'Bursa İnegöl OSB No:48', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Ereğli Enerji Ltd.', '1353536363', 4, 6, 'Konya Ereğli OSB No:15', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Alanya Gıda A.Ş.', '1363637373', 5, 7, 'Antalya Alanya OSB No:32', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Tarsus İlaç Ltd.', '1373738383', 3, 8, 'Adana Tarsus OSB No:9', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Nizip Ambalaj A.Ş.', '1383839393', 8, 9, 'Gaziantep Nizip OSB No:46', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Derince Sanayi Ltd.', '1393940404', 2, 10, 'Kocaeli Derince OSB No:24', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Acıpayam Kimya A.Ş.', '1404041414', 7, 11, 'Denizli Acıpayam OSB No:36', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çifteler Tekstil Ltd.', '1414142424', 6, 12, 'Eskişehir Çifteler OSB No:20', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Hendek Gıda A.Ş.', '1424243434', 5, 13, 'Sakarya Hendek OSB No:43', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Nazilli Enerji Ltd.', '1434344444', 4, 14, 'Aydın Nazilli OSB No:10', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Edremit Ambalaj A.Ş.', '1444445454', 8, 15, 'Balıkesir Edremit OSB No:51', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Turgutlu Sanayi Ltd.', '1454546464', 2, 1, 'Manisa Turgutlu OSB No:39', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Esenyurt İlaç A.Ş.', '1464647474', 3, 2, 'İstanbul Esenyurt OSB No:26', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çankaya Kimya Ltd.', '1474748484', 7, 3, 'Ankara Çankaya Teknokent No:7', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Bornova Tekstil A.Ş.', '1484849494', 6, 4, 'İzmir Bornova OSB No:34', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Yıldırım Gıda Ltd.', '1494950505', 5, 5, 'Bursa Yıldırım OSB No:47', 2);
                INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Selçuklu Enerji A.Ş.', '1505051515', 4, 6, 'Konya Selçuklu OSB No:30', 2);

                -- ===================== 50 ÇİFTLİK =====================
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Bereketli Topraklar Çiftliği', '2010101010', 1, 1, 'Salihli İlçesi Merkez Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Trakya Organik Tarım', '2020202020', 1, 2, 'Silivri Kavaklı Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Başkent Tarım İşletmesi', '2030303030', 1, 3, 'Polatlı Yenimahalle Mevkii', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Ege Zeytincilik A.Ş.', '2040404040', 1, 4, 'Tire Merkez Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Uludağ Yayla Çiftliği', '2050505050', 1, 5, 'Karacabey Ovası Mevkii', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Konya Ovası Tarım', '2060606060', 1, 6, 'Çumra İlçesi Merkez', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akdeniz Sera Çiftliği', '2070707070', 1, 7, 'Kumluca İlçesi Sahil Mevkii', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çukurova Pamuk Çiftliği', '2080808080', 1, 8, 'Ceyhan İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('GAP Buğday Çiftliği', '2090909090', 1, 9, 'Nizip İlçesi Merkez', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Marmara Süt Çiftliği', '2101010101', 1, 10, 'Kandıra İlçesi Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Pamukkale Organik Tarım', '2111111111', 1, 11, 'Çivril İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Sakarya Fındık Bahçeleri', '2121212121', 1, 12, 'Çifteler İlçesi Merkez', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Sapanca Meyve Bahçeleri', '2131313131', 1, 13, 'Sapanca Gölü Mevkii', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Aydın İncir Bahçeleri', '2141414141', 1, 14, 'Nazilli İlçesi Merkez', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Gönen Pirinç Çiftliği', '2151515151', 1, 15, 'Gönen İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akhisar Zeytin Çiftliği', '2161616161', 1, 1, 'Akhisar İlçesi Zeytinlik', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çatalca Hayvancılık', '2171717171', 1, 2, 'Çatalca İlçesi Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Haymana Buğday Tarlaları', '2181818181', 1, 3, 'Haymana İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Menemen Sebze Çiftliği', '2191919191', 1, 4, 'Menemen İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Mustafakemalpaşa Çiftliği', '2202020202', 1, 5, 'Mustafakemalpaşa Merkez', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Cihanbeyli Tarım', '2212121212', 1, 6, 'Cihanbeyli İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Serik Narenciye Bahçesi', '2222222222', 1, 7, 'Serik İlçesi Sahil', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Kozan Pamuk Çiftliği', '2232323232', 1, 8, 'Kozan İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Şahinbey Tahıl Çiftliği', '2242424242', 1, 9, 'Şahinbey İlçesi', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Kartepe Süt Çiftliği', '2252525252', 1, 10, 'Kartepe İlçesi Köyü', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Tavas Organik Çiftlik', '2262626262', 1, 11, 'Tavas İlçesi Merkez', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Sivrihisar Tarım', '2272727272', 1, 12, 'Sivrihisar İlçesi', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akyazı Hayvancılık', '2282828282', 1, 13, 'Akyazı İlçesi Köyü', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Germencik Zeytin Çiftliği', '2292929292', 1, 14, 'Germencik İlçesi', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Bandırma Tarım İşletmesi', '2303030303', 1, 15, 'Bandırma İlçesi Ovası', 1);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Soma Bağcılık Çiftliği', '2313131313', 1, 1, 'Soma İlçesi Bağlar', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Büyükçekmece Sera', '2323232323', 1, 2, 'Büyükçekmece Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Çubuk Sebze Üretimi', '2333333333', 1, 3, 'Çubuk İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Ödemiş Kiraz Bahçeleri', '2343434343', 1, 4, 'Ödemiş İlçesi Dağ', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Orhangazi Zeytin Çiftliği', '2353535353', 1, 5, 'Orhangazi İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Akşehir Tahıl Çiftliği', '2363636363', 1, 6, 'Akşehir İlçesi Ovası', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Manavgat Muz Plantasyonu', '2373737373', 1, 7, 'Manavgat İlçesi Sahil', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('İmamoğlu Pamuk Üretimi', '2383838383', 1, 8, 'İmamoğlu İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Araban Fıstık Bahçeleri', '2393939393', 1, 9, 'Araban İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Gölcük Tarım Çiftliği', '2404040404', 1, 10, 'Gölcük İlçesi Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Honaz Elma Bahçeleri', '2414141414', 1, 11, 'Honaz İlçesi Dağ', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Mihalıççık Hayvancılık', '2424242424', 1, 12, 'Mihalıççık İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Pamukova Meyve Çiftliği', '2434343434', 1, 13, 'Pamukova İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Kuşadası Üzüm Bağları', '2444444444', 1, 14, 'Kuşadası İlçesi Yamaç', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Susurluk Süt Çiftliği', '2454545454', 1, 15, 'Susurluk İlçesi Köyü', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Saruhanlı Bağcılık', '2464646464', 1, 1, 'Saruhanlı İlçesi Bağlar', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Arnavutköy Sera Üretimi', '2474747474', 1, 2, 'Arnavutköy İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Beypazarı Organik Tarım', '2484848484', 1, 3, 'Beypazarı İlçesi', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Bayındır Çiçek Çiftliği', '2494949494', 1, 4, 'Bayındır İlçesi Sera', 2);
                INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) VALUES ('Gemlik Zeytin Kooperatifi', '2505050505', 1, 5, 'Gemlik İlçesi Zeytinlik', 2);

                -- ===================== ÇİFTLİK ÜRÜNLERİ (100+ ürün) =====================
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (1, 1, 'Buğday Samanı', 250.5, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (1, 7, 'Mısır Koçanı', 180.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (2, 1, 'Organik Gübre', 320.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (2, 6, 'Ayçiçeği Sapı', 150.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (3, 7, 'Arpa Samanı', 200.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (3, 1, 'Hayvan Gübresi', 450.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (4, 1, 'Zeytin Posası', 85.5, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (4, 7, 'Zeytin Yaprağı', 45.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (5, 6, 'Çeltik Kabuğu', 175.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (5, 1, 'Sığır Gübresi', 280.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (6, 7, 'Buğday Samanı', 520.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (6, 6, 'Mısır Sapı', 380.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (7, 1, 'Sera Atığı', 95.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (7, 7, 'Domates Sapı', 65.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (8, 6, 'Pamuk Sapı', 420.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (8, 1, 'Pamuk Çiğidi', 180.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (9, 7, 'Buğday Samanı', 650.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (9, 6, 'Mercimek Sapı', 120.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (10, 1, 'Süt Çiftliği Gübresi', 380.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (10, 7, 'Yonca Artığı', 95.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (11, 6, 'Pamuk Atığı', 210.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (12, 1, 'Fındık Kabuğu', 85.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (13, 7, 'Elma Posası', 55.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (14, 1, 'İncir Yaprağı', 35.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (15, 6, 'Pirinç Kabuğu', 290.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (16, 1, 'Zeytin Posası', 125.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (17, 7, 'Hayvan Gübresi', 480.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (18, 6, 'Buğday Kavuzu', 340.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (19, 1, 'Sebze Atığı', 75.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (20, 7, 'Sığır Gübresi', 520.0, 2);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (21, 6, 'Arpa Samanı', 280.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (22, 1, 'Narenciye Kabuğu', 45.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (23, 7, 'Pamuk Lifi Atığı', 165.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (24, 6, 'Tahıl Kabuğu', 195.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (25, 1, 'Tavuk Gübresi', 310.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (26, 7, 'Organik Kompost', 220.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (27, 6, 'Yulaf Samanı', 145.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (28, 1, 'Koyun Gübresi', 270.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (29, 7, 'Zeytin Dalı', 55.0, 1);
                INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) VALUES (30, 6, 'Pirinç Samanı', 385.0, 1);

                -- ===================== ALIM TALEPLERİ (30 talep) =====================
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (1, 1, 1, 100.0, 'Enerji üretimi için saman gerekiyor', 2, '2024-11-15');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (2, 2, 3, 150.0, 'Organik gübre talep ediyoruz', 2, '2024-11-18');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (3, 3, 5, 80.0, 'Biyokütle için arpa samanı', 2, '2024-11-20');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (4, 4, 7, 40.0, 'Zeytin posası talebi', 2, '2024-11-22');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (5, 5, 9, 120.0, 'Çeltik kabuğu için teklif', 2, '2024-11-25');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (6, 6, 11, 200.0, 'Biyogaz tesisi için saman', 2, '2024-11-28');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (7, 7, 13, 50.0, 'Sera atığı kompost için', 2, '2024-12-01');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (8, 8, 15, 180.0, 'Pamuk sapı talebi', 2, '2024-12-03');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (9, 9, 17, 250.0, 'Buğday samanı biyokütle için', 2, '2024-12-05');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (10, 10, 19, 150.0, 'Gübre talebi organik tarım', 2, '2024-12-08');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (11, 11, 21, 90.0, 'Pamuk atığı geri dönüşüm', 1, '2024-12-10');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (12, 12, 22, 35.0, 'Fındık kabuğu yakacak', 1, '2024-12-11');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (13, 13, 23, 25.0, 'Elma posası kompost için', 1, '2024-12-12');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (14, 14, 24, 15.0, 'İncir yaprağı hayvan yemi', 1, '2024-12-13');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (15, 15, 25, 100.0, 'Pirinç kabuğu enerji üretimi', 1, '2024-12-14');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (16, 1, 2, 75.0, 'Mısır koçanı biyokütle', 1, '2024-12-15');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (17, 2, 4, 80.0, 'Ayçiçeği sapı talebi', 1, '2024-12-16');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (18, 3, 6, 200.0, 'Hayvan gübresi organik', 1, '2024-12-17');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (19, 4, 8, 20.0, 'Zeytin yaprağı yem katkısı', 1, '2024-12-18');
                INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID, TalepTarihi) VALUES (20, 5, 10, 130.0, 'Sığır gübresi biyogaz', 1, '2024-12-19');

                -- ===================== SDG RAPOR VERİLERİ =====================
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (1, 100.0, 45.0, 125000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (2, 150.0, 67.5, 187500.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (3, 80.0, 36.0, 100000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (4, 40.0, 18.0, 50000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (5, 120.0, 54.0, 150000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (6, 200.0, 90.0, 250000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (7, 50.0, 22.5, 62500.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (8, 180.0, 81.0, 225000.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (9, 250.0, 112.5, 312500.00);
                INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) VALUES (10, 150.0, 67.5, 187500.00);

                -- ===================== İŞLEM LOGLARI =====================
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Sistem başlatıldı - Yeşil Eksen v1.0', 0);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Anadolu Enerji A.Ş. firması onaylandı', 3);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Marmara Gıda San. Ltd. firması onaylandı', 3);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Başkent İlaç A.Ş. alım talebi onaylandı', 3);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Ege Kimya 100 ton saman talebi onaylandı', 3);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('GAP Enerji biyokütle siparişi tamamlandı', 3);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Bereketli Topraklar Çiftliği onaylandı', 4);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Trakya Organik Tarım onaylandı', 4);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Ege Zeytincilik 85 ton zeytin posası ürünü onaylandı', 4);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Konya Ovası Tarım buğday samanı ürünü onaylandı', 4);
                INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES ('Çukurova Pamuk Çiftliği pamuk atığı kaydedildi', 4);
                ";

                using (var cmd = new SQLiteCommand(insertSQL, conn))
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
            // Session'dan RolID al, yoksa 0 kullan
            int rolID = Session.RolID > 0 ? Session.RolID : 0;
            string query = "INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES (@aciklama, @rolID)";
            ExecuteNonQuery(query, 
                new SQLiteParameter("@aciklama", aciklama),
                new SQLiteParameter("@rolID", rolID));
        }

        public static void LogIslem(string aciklama, int rolID)
        {
            string query = "INSERT INTO Tbl_IslemLoglari (Aciklama, RolID) VALUES (@aciklama, @rolID)";
            ExecuteNonQuery(query, 
                new SQLiteParameter("@aciklama", aciklama),
                new SQLiteParameter("@rolID", rolID));
        }
    }
}

