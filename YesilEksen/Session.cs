using System;

namespace YesilEksen
{
    /// <summary>
    /// Oturum açmış kullanıcının bilgilerini tutar
    /// </summary>
    public static class Session
    {
        // Kullanıcı bilgileri
        public static int KullaniciID { get; set; }
        public static string KullaniciAdi { get; set; }
        public static int RolID { get; set; }
        public static string RolAdi { get; set; }
        public static int? IlgiliID { get; set; } // FirmaID veya CiftlikID

        /// <summary>
        /// Oturum bilgilerini temizler
        /// </summary>
        public static void Clear()
        {
            KullaniciID = 0;
            KullaniciAdi = string.Empty;
            RolID = 0;
            RolAdi = string.Empty;
            IlgiliID = null;
        }

        /// <summary>
        /// Kullanıcının Sanayi Odası Admin olup olmadığını kontrol eder
        /// </summary>
        public static bool IsSanayiAdmin => RolID == 3;

        /// <summary>
        /// Kullanıcının Ziraat Odası Admin olup olmadığını kontrol eder
        /// </summary>
        public static bool IsZiraatAdmin => RolID == 4;

        /// <summary>
        /// Kullanıcının Firma olup olmadığını kontrol eder
        /// </summary>
        public static bool IsFirma => RolID == 1;

        /// <summary>
        /// Kullanıcının Çiftlik olup olmadığını kontrol eder
        /// </summary>
        public static bool IsCiftlik => RolID == 2;
    }
}

