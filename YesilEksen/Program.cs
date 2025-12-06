using System;
using System.Windows.Forms;
using OfficeOpenXml;

namespace YesilEksen
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // EPPlus 8 ve sonrası için lisans ayarı (uygulama başlangıcında)
            // EPPlus 8'de LicenseContext yerine License.SetNonCommercialPersonal() veya SetNonCommercialOrganization() kullanılıyor
            ExcelPackage.License.SetNonCommercialPersonal("YesilEksen");
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Uygulama Login ekranı ile başlar
            Application.Run(new Login());
        }
    }
}
