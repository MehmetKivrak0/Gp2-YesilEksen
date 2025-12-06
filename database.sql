-- =============================================
-- PROJE: Yeşil Eksen
-- VERİTABANI: SQLite
-- AÇIKLAMA: Bu script tüm tabloları sıfırdan oluşturur ve test verilerini ekler.
-- =============================================

BEGIN TRANSACTION;

-- 1. ADIM: Önce eski tabloları (varsa) temizle
DROP TABLE IF EXISTS Tbl_SdgRaporVerisi;
DROP TABLE IF EXISTS Tbl_IslemLoglari;
DROP TABLE IF EXISTS Tbl_AlimTalepleri;
DROP TABLE IF EXISTS Tbl_CiftlikUrunleri;
DROP TABLE IF EXISTS Tbl_Kullanicilar;
DROP TABLE IF EXISTS Tbl_CiftlikBelgeleri;
DROP TABLE IF EXISTS Tbl_FirmaBelgeleri;
DROP TABLE IF EXISTS Tbl_UrunBelgeleri;
DROP TABLE IF EXISTS Tbl_Ciftlikler;
DROP TABLE IF EXISTS Tbl_Firmalar;
DROP TABLE IF EXISTS Tbl_Roller;
DROP TABLE IF EXISTS Tbl_OnayDurumlari;
DROP TABLE IF EXISTS Tbl_UrunKategorileri;
DROP TABLE IF EXISTS Tbl_Sektorler;
DROP TABLE IF EXISTS Tbl_Sehirler;

-- 2. ADIM: Sabit (Lookup) Tabloları Oluştur
CREATE TABLE Tbl_Sehirler (
    SehirID INTEGER PRIMARY KEY AUTOINCREMENT,
    SehirAdi TEXT
);

CREATE TABLE Tbl_Sektorler (
    SektorID INTEGER PRIMARY KEY AUTOINCREMENT,
    SektorAdi TEXT
);

CREATE TABLE Tbl_UrunKategorileri (
    KategoriID INTEGER PRIMARY KEY AUTOINCREMENT,
    KategoriAdi TEXT
);

CREATE TABLE Tbl_OnayDurumlari (
    DurumID INTEGER PRIMARY KEY AUTOINCREMENT,
    DurumAdi TEXT
);

CREATE TABLE Tbl_Roller (
    RolID INTEGER PRIMARY KEY AUTOINCREMENT,
    RolAdi TEXT -- 'Firma', 'Ciftlik', 'Sanayi Odası Admin', 'Ziraat Odası Admin'
);

-- 3. ADIM: Ana Aktör Tabloları
CREATE TABLE Tbl_Firmalar (
    FirmaID INTEGER PRIMARY KEY AUTOINCREMENT,
    Unvan TEXT,           -- nvarchar(250)
    VergiNo TEXT,         -- varchar(11)
    SektorID INTEGER,     -- FK
    SehirID INTEGER,      -- FK
    Adres TEXT,           -- nvarchar(MAX)
    LogoUrl TEXT,         -- nvarchar(500)
    DurumID INTEGER DEFAULT 1
);

CREATE TABLE Tbl_Ciftlikler (
    CiftlikID INTEGER PRIMARY KEY AUTOINCREMENT,
    Unvan TEXT,
    VergiNo TEXT,
    SektorID INTEGER,
    SehirID INTEGER,
    Adres TEXT,
    LogoUrl TEXT,
    DurumID INTEGER DEFAULT 1 -- Yeni eklenen çiftlik onaysız gelir
);

-- 4. ADIM: Kullanıcı Giriş Tablosu (Login İçin Kritik)
CREATE TABLE Tbl_Kullanicilar (
    KullaniciID INTEGER PRIMARY KEY AUTOINCREMENT,
    RolID INTEGER,        -- FK: Tbl_Roller
    KullaniciAdi TEXT,
    SifreHash TEXT,
    IlgiliID INTEGER,     -- FirmaID veya CiftlikID buraya yazılır
    DurumID INTEGER,      -- 1:Onay Bekliyor, 2:Aktif
    KayitTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 5. ADIM: Ürün ve Talep Yönetimi
CREATE TABLE Tbl_CiftlikUrunleri (
    UrunID INTEGER PRIMARY KEY AUTOINCREMENT,
    CiftlikID INTEGER,    -- FK
    UrunKategoriID INTEGER, -- FK
    UrunAdi TEXT,
    MiktarTon REAL,       -- decimal(10,2) karşılığı
    DurumID INTEGER
);

CREATE TABLE Tbl_AlimTalepleri (
    TalepID INTEGER PRIMARY KEY AUTOINCREMENT,
    FirmaID INTEGER,          -- Talep eden firma
    HedefCiftlikID INTEGER,   -- Hangi çiftlikten isteniyor
    UrunID INTEGER,           -- Hangi ürün
    TalepMiktarTon REAL,
    FirmaNotu TEXT,
    DurumID INTEGER,
    ReddetmeNedeni TEXT,      -- Sanayi odası reddederse buraya yazar
    TalepTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 6. ADIM: Belge Yönetimi (Görsellerdeki Belge Tabloları)
CREATE TABLE Tbl_CiftlikBelgeleri (
    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
    CiftlikID INTEGER,
    BelgeAdi TEXT,
    DosyaYolu TEXT
);

CREATE TABLE Tbl_FirmaBelgeleri (
    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
    FirmaID INTEGER,
    BelgeAdi TEXT,
    DosyaYolu TEXT
);

CREATE TABLE Tbl_UrunBelgeleri (
    BelgeID INTEGER PRIMARY KEY AUTOINCREMENT,
    UrunID INTEGER,
    BelgeAdi TEXT,
    DosyaYolu TEXT
);

-- 7. ADIM: Raporlama ve Loglar
CREATE TABLE Tbl_IslemLoglari (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    Aciklama TEXT,
    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Tbl_SdgRaporVerisi (
    RaporVeriID INTEGER PRIMARY KEY AUTOINCREMENT,
    OnaylananTalepID INTEGER, -- Hangi satış işlemi raporlandı
    GeriKazanilanAtikTon REAL,
    EngellenenCO2Ton REAL,
    EkonomikDegerTL REAL,
    IslemTarihi DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 8. ADIM: TEST VERİLERİNİ EKLE
-- =============================================

-- Şehirler
INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Manisa');
INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('İstanbul');
INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Ankara');
INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('İzmir');
INSERT INTO Tbl_Sehirler (SehirAdi) VALUES ('Bursa');

-- Sektörler
INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Tarım');
INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Sanayi');
INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('İlaç');
INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Enerji');
INSERT INTO Tbl_Sektorler (SektorAdi) VALUES ('Gıda');

-- Ürün Kategorileri
INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Organik Atık');
INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Plastik Atık');
INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Metal Atık');
INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Cam Atık');
INSERT INTO Tbl_UrunKategorileri (KategoriAdi) VALUES ('Kağıt Atık');

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
INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) 
VALUES (3, 'sanayi_admin', '123456', NULL, 2);
INSERT INTO Tbl_Kullanicilar (RolID, KullaniciAdi, SifreHash, IlgiliID, DurumID) 
VALUES (4, 'ziraat_admin', '123456', NULL, 2);

-- Test Firmaları
INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Yeşil Enerji A.Ş.', '1234567890', 4, 1, 'Organize Sanayi Bölgesi No:15', 1);
INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Eko Gıda Ltd.', '9876543210', 5, 2, 'Beylikdüzü Sanayi Sitesi', 2);
INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Bio İlaç A.Ş.', '1112223334', 3, 3, 'Teknokent Binası Kat:3', 1);
INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Temiz Sanayi Ltd.', '4445556667', 2, 4, 'Atatürk OSB No:22', 2);
INSERT INTO Tbl_Firmalar (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Doğa Enerji A.Ş.', '7778889990', 4, 5, 'DOSAB 5. Cadde No:8', 1);

-- Test Çiftlikleri
INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Bereket Çiftliği', '1111111111', 1, 1, 'Salihli Köyü Mevkii', 1);
INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Verimli Tarım', '2222222222', 1, 2, 'Silivri Tarım Alanı', 2);
INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Organik Bahçe', '3333333333', 1, 3, 'Polatlı Ovası', 1);
INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Yeşil Vadi Çiftliği', '4444444444', 1, 4, 'Tire Bölgesi', 2);
INSERT INTO Tbl_Ciftlikler (Unvan, VergiNo, SektorID, SehirID, Adres, DurumID) 
VALUES ('Altın Başak', '5555555555', 1, 5, 'Karacabey Ovası', 1);

-- Çiftlik Ürünleri
INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) 
VALUES (1, 1, 'Saman Atığı', 150.5, 2);
INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) 
VALUES (1, 1, 'Mısır Sapı', 80.0, 1);
INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) 
VALUES (2, 1, 'Organik Gübre', 200.0, 2);
INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) 
VALUES (3, 1, 'Buğday Samanı', 120.0, 1);
INSERT INTO Tbl_CiftlikUrunleri (CiftlikID, UrunKategoriID, UrunAdi, MiktarTon, DurumID) 
VALUES (4, 1, 'Zeytin Posası', 50.5, 2);

-- Alım Talepleri
INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID) 
VALUES (1, 1, 1, 50.0, 'Enerji üretimi için saman gerekiyor', 1);
INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID) 
VALUES (2, 2, 3, 100.0, 'Organik gübre talep ediyoruz', 2);
INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID) 
VALUES (3, 3, 4, 30.0, 'Biyokütle için buğday samanı', 1);
INSERT INTO Tbl_AlimTalepleri (FirmaID, HedefCiftlikID, UrunID, TalepMiktarTon, FirmaNotu, DurumID) 
VALUES (4, 4, 5, 25.0, 'Zeytin posası talebi', 3);

-- SDG Rapor Verileri
INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) 
VALUES (2, 100.0, 45.5, 125000.00);
INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) 
VALUES (NULL, 75.0, 33.8, 95000.00);
INSERT INTO Tbl_SdgRaporVerisi (OnaylananTalepID, GeriKazanilanAtikTon, EngellenenCO2Ton, EkonomikDegerTL) 
VALUES (NULL, 200.0, 90.0, 250000.00);

-- Firma Belgeleri
INSERT INTO Tbl_FirmaBelgeleri (FirmaID, BelgeAdi, DosyaYolu)
VALUES (1, 'Vergi Levhası', 'belgeler/firma1_vergi.pdf');
INSERT INTO Tbl_FirmaBelgeleri (FirmaID, BelgeAdi, DosyaYolu)
VALUES (1, 'Ticaret Sicil Belgesi', 'belgeler/firma1_sicil.pdf');
INSERT INTO Tbl_FirmaBelgeleri (FirmaID, BelgeAdi, DosyaYolu)
VALUES (3, 'Vergi Levhası', 'belgeler/firma3_vergi.pdf');

-- Çiftlik Belgeleri
INSERT INTO Tbl_CiftlikBelgeleri (CiftlikID, BelgeAdi, DosyaYolu)
VALUES (1, 'Çiftçi Kayıt Belgesi', 'belgeler/ciftlik1_kayit.pdf');
INSERT INTO Tbl_CiftlikBelgeleri (CiftlikID, BelgeAdi, DosyaYolu)
VALUES (3, 'Organik Sertifika', 'belgeler/ciftlik3_organik.pdf');

-- İşlem Logları
INSERT INTO Tbl_IslemLoglari (Aciklama) VALUES ('Sistem başlatıldı');
INSERT INTO Tbl_IslemLoglari (Aciklama) VALUES ('Eko Gıda Ltd. firması onaylandı');
INSERT INTO Tbl_IslemLoglari (Aciklama) VALUES ('Verimli Tarım çiftliği onaylandı');
INSERT INTO Tbl_IslemLoglari (Aciklama) VALUES ('Organik gübre talebi (100 Ton) onaylandı');
INSERT INTO Tbl_IslemLoglari (Aciklama) VALUES ('Zeytin posası talebi reddedildi - Stok yetersiz');

COMMIT;
