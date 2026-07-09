using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public class ReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IExcelExportService _excelExportService;

        public ReportsController(IMediator mediator, IExcelExportService excelExportService)
        {
            _mediator = mediator;
            _excelExportService = excelExportService;
        }

        // ─── TURNOS ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Exporta los turnos a un archivo Excel (.xlsx).
        /// </summary>
        [HttpGet("appointments/excel")]
        public async Task<IActionResult> ExportAppointmentsExcel(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] int? gestorId)
        {
            var data = await _mediator.Send(new GetAppointmentsReportQuery
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                GestorFarmaceuticoId = gestorId
            });

            var bytes = _excelExportService.ExportAppointmentsToExcel(data);
            var fileName = $"Turnos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// Genera un PDF con el reporte de turnos usando Rotativa.
        /// </summary>
        [HttpGet("appointments/pdf")]
        public async Task<IActionResult> ExportAppointmentsPdf(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] int? gestorId)
        {
            var data = await _mediator.Send(new GetAppointmentsReportQuery
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                GestorFarmaceuticoId = gestorId
            });

            var fileName = $"Turnos_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return new ViewAsPdf("AppointmentReport", data)
            {
                FileName = fileName,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 15, 10, 15),
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        // ─── INVENTARIO ───────────────────────────────────────────────────────────

        /// <summary>
        /// Exporta el inventario a un archivo Excel (.xlsx).
        /// </summary>
        [HttpGet("inventory/excel")]
        public async Task<IActionResult> ExportInventoryExcel(
            [FromQuery] int? gestorId,
            [FromQuery] bool? onlyActive,
            [FromQuery] bool? onlyLowStock)
        {
            var data = await _mediator.Send(new GetInventoryReportQuery
            {
                GestorFarmaceuticoId = gestorId,
                OnlyActive = onlyActive,
                OnlyLowStock = onlyLowStock
            });

            var bytes = _excelExportService.ExportInventoryToExcel(data);
            var fileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// Genera un PDF con el reporte de inventario usando Rotativa.
        /// </summary>
        [HttpGet("inventory/pdf")]
        public async Task<IActionResult> ExportInventoryPdf(
            [FromQuery] int? gestorId,
            [FromQuery] bool? onlyActive,
            [FromQuery] bool? onlyLowStock)
        {
            var data = await _mediator.Send(new GetInventoryReportQuery
            {
                GestorFarmaceuticoId = gestorId,
                OnlyActive = onlyActive,
                OnlyLowStock = onlyLowStock
            });

            var fileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return new ViewAsPdf("InventoryReport", data)
            {
                FileName = fileName,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 15, 10, 15),
                CustomSwitches = "--disable-smart-shrinking"
            };
        }
    }
}
