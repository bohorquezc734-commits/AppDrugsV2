using ClosedXML.Excel;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.DTOs;

namespace AppDrugsV2.Infrastructure.Services.Reports
{
    public class ExcelExportService : IExcelExportService
    {
        // Colores corporativos
        private static readonly XLColor HeaderBg       = XLColor.FromHtml("#1E3A5F");  // azul marino
        private static readonly XLColor HeaderFg       = XLColor.White;
        private static readonly XLColor AltRowBg       = XLColor.FromHtml("#F0F4FA");  // gris azulado
        private static readonly XLColor LowStockBg     = XLColor.FromHtml("#FEE2E2");  // rojo suave
        private static readonly XLColor OkStockBg      = XLColor.FromHtml("#D1FAE5");  // verde suave
        private static readonly XLColor EmptyStockBg   = XLColor.FromHtml("#E5E7EB");  // gris

        // ─── Turnos ───────────────────────────────────────────────────────────────

        public byte[] ExportAppointmentsToExcel(IEnumerable<AppointmentReportDto> data)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Turnos");

            // --- Título ---
            var titleCell = ws.Cell(1, 1);
            titleCell.Value = "Reporte de Turnos — AppDrugsV2";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            titleCell.Style.Font.FontColor = XLColor.FromHtml("#1E3A5F");
            ws.Range(1, 1, 1, 8).Merge();

            var genCell = ws.Cell(2, 1);
            genCell.Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            genCell.Style.Font.Italic = true;
            genCell.Style.Font.FontSize = 9;
            genCell.Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 8).Merge();

            // --- Encabezados (fila 4) ---
            var headers = new[]
            {
                "N°", "Usuario", "Sede", "Estado",
                "Fecha Creación", "Fecha Entrega", "Archivo Adjunto", "Medicamentos"
            };
            SetHeaders(ws, row: 4, headers);

            // --- Datos ---
            var list = data.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                int row = i + 5;
                var dto = list[i];

                ws.Cell(row, 1).Value = dto.Id;
                ws.Cell(row, 2).Value = dto.NombreUsuario;
                ws.Cell(row, 3).Value = dto.Sede;
                ws.Cell(row, 4).Value = dto.Estado;
                ws.Cell(row, 5).Value = dto.FechaCreacion;
                ws.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                if (dto.FechaEntrega.HasValue)
                {
                    ws.Cell(row, 6).Value = dto.FechaEntrega.Value;
                    ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                }
                else
                {
                    ws.Cell(row, 6).Value = "—";
                }
                ws.Cell(row, 7).Value = dto.ArchivoAdjunto ?? "—";
                ws.Cell(row, 8).Value = dto.MedicamentosTexto;

                // Color de estado
                var estadoColor = dto.Estado switch
                {
                    "Recibido"  => XLColor.FromHtml("#DBEAFE"),
                    "EnProceso" => XLColor.FromHtml("#FEF3C7"),
                    "Entregado" => XLColor.FromHtml("#D1FAE5"),
                    "Cancelado" => XLColor.FromHtml("#FEE2E2"),
                    _           => XLColor.White
                };
                ws.Cell(row, 4).Style.Fill.BackgroundColor = estadoColor;

                // Filas alternas
                if (i % 2 == 0)
                    ApplyAltRow(ws, row, 8, AltRowBg);

                // Borde completo
                ws.Range(row, 1, row, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row, 8).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            }

            // --- Formato final ---
            ws.Columns().AdjustToContents();
            ws.Column(8).Width = Math.Min(ws.Column(8).Width, 50); // max ancho medicamentos
            ws.SheetView.FreezeRows(4);
            ws.AutoFilter.IsEnabled = true;
            ws.AutoFilter.Range = ws.Range(4, 1, list.Count + 4, 8);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ─── Inventario ───────────────────────────────────────────────────────────

        public byte[] ExportInventoryToExcel(IEnumerable<InventoryReportDto> data)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario");

            // --- Título ---
            var titleCell = ws.Cell(1, 1);
            titleCell.Value = "Reporte de Inventario — AppDrugsV2";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            titleCell.Style.Font.FontColor = XLColor.FromHtml("#1E3A5F");
            ws.Range(1, 1, 1, 7).Merge();

            var genCell = ws.Cell(2, 1);
            genCell.Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            genCell.Style.Font.Italic = true;
            genCell.Style.Font.FontSize = 9;
            genCell.Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 7).Merge();

            // --- Encabezados ---
            var headers = new[]
            {
                "N°", "Medicamento", "Sede", "Stock",
                "Estado Stock", "Estado Registro", "Última Actualización"
            };
            SetHeaders(ws, row: 4, headers);

            // --- Datos ---
            var list = data.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                int row = i + 5;
                var dto = list[i];

                ws.Cell(row, 1).Value = dto.Id;
                ws.Cell(row, 2).Value = dto.Medicamento;
                ws.Cell(row, 3).Value = dto.Sede;
                ws.Cell(row, 4).Value = dto.Stock;
                ws.Cell(row, 5).Value = dto.EstadoStock;
                ws.Cell(row, 6).Value = dto.EstadoTexto;
                if (dto.FechaActualizacion.HasValue)
                {
                    ws.Cell(row, 7).Value = dto.FechaActualizacion.Value;
                    ws.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                }
                else
                {
                    ws.Cell(row, 7).Value = "—";
                }

                // Color según stock
                var stockBg = dto.Stock == 0 ? EmptyStockBg
                            : dto.Stock < 10  ? LowStockBg
                                              : OkStockBg;
                ws.Cell(row, 4).Style.Fill.BackgroundColor = stockBg;
                ws.Cell(row, 5).Style.Fill.BackgroundColor = stockBg;
                ws.Cell(row, 4).Style.Font.Bold = dto.Stock < 10;

                // Filas alternas en columnas no coloreadas
                if (i % 2 == 0)
                    foreach (int col in new[] { 1, 2, 3, 6, 7 })
                        ws.Cell(row, col).Style.Fill.BackgroundColor = AltRowBg;

                ws.Range(row, 1, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            }

            // --- Resumen al final ---
            int summaryRow = list.Count + 6;
            ws.Cell(summaryRow, 1).Value = "Totales:";
            ws.Cell(summaryRow, 1).Style.Font.Bold = true;
            ws.Cell(summaryRow, 4).FormulaA1 = $"=SUM(D5:D{list.Count + 4})";
            ws.Cell(summaryRow, 4).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(4);
            ws.AutoFilter.IsEnabled = true;
            ws.AutoFilter.Range = ws.Range(4, 1, list.Count + 4, 7);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static void SetHeaders(IXLWorksheet ws, int row, string[] headers)
        {
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(row, col);
                cell.Value = headers[col - 1];
                cell.Style.Fill.BackgroundColor = HeaderBg;
                cell.Style.Font.FontColor = HeaderFg;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.White;
            }
        }

        private static void ApplyAltRow(IXLWorksheet ws, int row, int colCount, XLColor color)
        {
            for (int col = 1; col <= colCount; col++)
            {
                if (ws.Cell(row, col).Style.Fill.BackgroundColor == XLColor.White
                    || ws.Cell(row, col).Style.Fill.BackgroundColor == XLColor.NoColor)
                    ws.Cell(row, col).Style.Fill.BackgroundColor = color;
            }
        }
    }
}
