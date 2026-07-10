using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
    public class ReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IExcelExportService _excelExportService;

        public ReportsController(IMediator mediator, IExcelExportService excelExportService)
        {
            _mediator            = mediator;
            _excelExportService  = excelExportService;
        }

        // ─── TURNOS ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Exporta los turnos a un archivo Excel (.xlsx).
        /// </summary>
        [HttpGet("appointments/excel")]
        public async Task<IActionResult> ExportAppointmentsExcel(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string?   status,
            [FromQuery] int?      gestorId)
        {
            var data = await _mediator.Send(new GetAppointmentsReportQuery
            {
                DateFrom             = dateFrom,
                DateTo               = dateTo,
                Status               = status,
                GestorFarmaceuticoId = gestorId
            });

            var bytes    = _excelExportService.ExportAppointmentsToExcel(data);
            var fileName = $"{AppConstants.Rotativa.FileNames.AppointmentsPrefix}_{DateTime.Now.ToString(AppConstants.Rotativa.FileNames.DateFormat)}.xlsx";

            return File(bytes, AppConstants.ContentTypes.Excel, fileName);
        }

        /// <summary>
        /// Genera un PDF con el reporte de turnos usando Rotativa.
        /// </summary>
        [HttpGet("appointments/pdf")]
        public async Task<IActionResult> ExportAppointmentsPdf(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string?   status,
            [FromQuery] int?      gestorId)
        {
            var data = await _mediator.Send(new GetAppointmentsReportQuery
            {
                DateFrom             = dateFrom,
                DateTo               = dateTo,
                Status               = status,
                GestorFarmaceuticoId = gestorId
            });

            var fileName = $"{AppConstants.Rotativa.FileNames.AppointmentsPrefix}_{DateTime.Now.ToString(AppConstants.Rotativa.FileNames.DateFormat)}.pdf";

            return new ViewAsPdf(AppConstants.Rotativa.Views.AppointmentReport, data)
            {
                FileName        = fileName,
                PageOrientation = Orientation.Landscape,
                PageSize        = Size.A4,
                PageMargins     = new Margins(
                    AppConstants.Rotativa.MarginTop,
                    AppConstants.Rotativa.MarginLeft,
                    AppConstants.Rotativa.MarginBottom,
                    AppConstants.Rotativa.MarginRight),
                CustomSwitches = AppConstants.Rotativa.DisableSmartShrinking
            };
        }

        // ─── INVENTARIO ───────────────────────────────────────────────────────────

        /// <summary>
        /// Exporta el inventario a un archivo Excel (.xlsx).
        /// </summary>
        [HttpGet("inventory/excel")]
        public async Task<IActionResult> ExportInventoryExcel(
            [FromQuery] int?  gestorId,
            [FromQuery] bool? onlyActive,
            [FromQuery] bool? onlyLowStock)
        {
            var data = await _mediator.Send(new GetInventoryReportQuery
            {
                GestorFarmaceuticoId = gestorId,
                OnlyActive           = onlyActive,
                OnlyLowStock         = onlyLowStock
            });

            var bytes    = _excelExportService.ExportInventoryToExcel(data);
            var fileName = $"{AppConstants.Rotativa.FileNames.InventoryPrefix}_{DateTime.Now.ToString(AppConstants.Rotativa.FileNames.DateFormat)}.xlsx";

            return File(bytes, AppConstants.ContentTypes.Excel, fileName);
        }

        /// <summary>
        /// Genera un PDF con el reporte de inventario usando Rotativa.
        /// </summary>
        [HttpGet("inventory/pdf")]
        public async Task<IActionResult> ExportInventoryPdf(
            [FromQuery] int?  gestorId,
            [FromQuery] bool? onlyActive,
            [FromQuery] bool? onlyLowStock)
        {
            var data = await _mediator.Send(new GetInventoryReportQuery
            {
                GestorFarmaceuticoId = gestorId,
                OnlyActive           = onlyActive,
                OnlyLowStock         = onlyLowStock
            });

            var fileName = $"{AppConstants.Rotativa.FileNames.InventoryPrefix}_{DateTime.Now.ToString(AppConstants.Rotativa.FileNames.DateFormat)}.pdf";

            return new ViewAsPdf(AppConstants.Rotativa.Views.InventoryReport, data)
            {
                FileName        = fileName,
                PageOrientation = Orientation.Landscape,
                PageSize        = Size.A4,
                PageMargins     = new Margins(
                    AppConstants.Rotativa.MarginTop,
                    AppConstants.Rotativa.MarginLeft,
                    AppConstants.Rotativa.MarginBottom,
                    AppConstants.Rotativa.MarginRight),
                CustomSwitches = AppConstants.Rotativa.DisableSmartShrinking
            };
        }
    }
}
