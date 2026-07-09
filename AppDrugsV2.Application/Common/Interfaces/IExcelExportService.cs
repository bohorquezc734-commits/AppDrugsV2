using AppDrugsV2.Application.Features.Reports.DTOs;

namespace AppDrugsV2.Application.Common.Interfaces
{
    public interface IExcelExportService
    {
        /// <summary>
        /// Exporta la lista de turnos a un archivo .xlsx en memoria.
        /// </summary>
        byte[] ExportAppointmentsToExcel(IEnumerable<AppointmentReportDto> data);

        /// <summary>
        /// Exporta la lista de inventario a un archivo .xlsx en memoria.
        /// </summary>
        byte[] ExportInventoryToExcel(IEnumerable<InventoryReportDto> data);
    }
}
