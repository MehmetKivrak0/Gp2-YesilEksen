using System;
using System.Data;
using System.IO;
using OfficeOpenXml;
using System.Drawing;

namespace YesilEksen
{
    /// <summary>
    /// Excel export helper sınıfı
    /// </summary>
    public static class ExcelHelper
    {

        /// <summary>
        /// DataTable'ı gerçek Excel (.xlsx) formatına aktarır
        /// </summary>
        public static void ExportToExcel(DataTable data, string filePath, string reportTitle, 
            string summaryLabel1 = "", string summaryValue1 = "",
            string summaryLabel2 = "", string summaryValue2 = "",
            string summaryLabel3 = "", string summaryValue3 = "")
        {

            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Rapor");
                int row = 1;

                // Başlık
                worksheet.Cells[row, 1].Value = $"YEŞİL EKSEN - {reportTitle}";
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;

                worksheet.Cells[row, 1].Value = $"Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";
                row++;

                worksheet.Cells[row, 1].Value = $"Oluşturan: {Session.KullaniciAdi}";
                row++;

                // Boş satır
                row++;

                // Özet bilgileri
                if (!string.IsNullOrEmpty(summaryLabel1))
                {
                    worksheet.Cells[row, 1].Value = "=== ÖZET BİLGİLER ===";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    worksheet.Cells[row, 1].Value = summaryLabel1;
                    worksheet.Cells[row, 2].Value = summaryValue1;
                    row++;

                    if (!string.IsNullOrEmpty(summaryLabel2))
                    {
                        worksheet.Cells[row, 1].Value = summaryLabel2;
                        worksheet.Cells[row, 2].Value = summaryValue2;
                        row++;
                    }

                    if (!string.IsNullOrEmpty(summaryLabel3))
                    {
                        worksheet.Cells[row, 1].Value = summaryLabel3;
                        worksheet.Cells[row, 2].Value = summaryValue3;
                        row++;
                    }

                    // Boş satır
                    row++;
                }

                if (data != null && data.Rows.Count > 0)
                {
                    // Detaylı veriler başlığı
                    worksheet.Cells[row, 1].Value = "=== DETAYLI VERİLER ===";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Başlık satırı
                    int col = 1;
                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        if (data.Columns[i].ColumnName != "ID")
                        {
                            worksheet.Cells[row, col].Value = data.Columns[i].ColumnName;
                            worksheet.Cells[row, col].Style.Font.Bold = true;
                            worksheet.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            col++;
                        }
                    }
                    row++;

                    // Veri satırları
                    foreach (DataRow dataRow in data.Rows)
                    {
                        col = 1;
                        for (int i = 0; i < data.Columns.Count; i++)
                        {
                            if (data.Columns[i].ColumnName != "ID")
                            {
                                worksheet.Cells[row, col].Value = dataRow[i]?.ToString() ?? "";
                                col++;
                            }
                        }
                        row++;
                    }

                    // Sütun genişliklerini otomatik ayarla
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                // Dosyayı kaydet
                FileInfo fileInfo = new FileInfo(filePath);
                excel.SaveAs(fileInfo);
            }
        }
    }
}

