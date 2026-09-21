using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.DTOs;
using AppDrugsV2.Application.Features.Reports.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class ReportsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IExcelExportService> _excelExportServiceMock;
        private ReportsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _excelExportServiceMock = new Mock<IExcelExportService>();
            _controller = new ReportsController(_mediatorMock.Object, _excelExportServiceMock.Object);
        }

        [Test]
        public async Task ExportAppointmentsExcel_ShouldReturnFile_WhenSuccessful()
        {
            var data = new List<AppointmentReportDto>();
            var fileBytes = new byte[] { 1, 2, 3 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentsReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _excelExportServiceMock.Setup(s => s.ExportAppointmentsToExcel(data))
                .Returns(fileBytes);

            var result = await _controller.ExportAppointmentsExcel(null, null, null, null);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.ContentType.Should().Be(AppConstants.ContentTypes.Excel);
            fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
        }

        [Test]
        public async Task ExportAppointmentsExcel_ShouldPassFilters_ToQuery()
        {
            var dateFrom = new DateTime(2025, 1, 1);
            var dateTo = new DateTime(2025, 12, 31);
            var data = new List<AppointmentReportDto>();
            _mediatorMock.Setup(m => m.Send(
                    It.Is<GetAppointmentsReportQuery>(q =>
                        q.DateFrom == dateFrom && q.DateTo == dateTo && q.Status == "Entregado" && q.GestorFarmaceuticoId == 5),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _excelExportServiceMock.Setup(s => s.ExportAppointmentsToExcel(data)).Returns(new byte[0]);

            var result = await _controller.ExportAppointmentsExcel(dateFrom, dateTo, "Entregado", 5);

            
            result.Should().BeOfType<FileContentResult>();
        }

        [Test]
        public async Task ExportInventoryExcel_ShouldReturnFile_WhenSuccessful()
        {
            var data = new List<InventoryReportDto>();
            var fileBytes = new byte[] { 4, 5, 6 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _excelExportServiceMock.Setup(s => s.ExportInventoryToExcel(data))
                .Returns(fileBytes);

            
            var result = await _controller.ExportInventoryExcel(null, null, null);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.ContentType.Should().Be(AppConstants.ContentTypes.Excel);
            fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
        }

        [Test]
        public async Task ExportInventoryExcel_ShouldPassFilters_ToQuery()
        {
            var data = new List<InventoryReportDto>();
            _mediatorMock.Setup(m => m.Send(
                    It.Is<GetInventoryReportQuery>(q =>
                        q.GestorFarmaceuticoId == 3 && q.OnlyActive == true && q.OnlyLowStock == false),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _excelExportServiceMock.Setup(s => s.ExportInventoryToExcel(data)).Returns(new byte[0]);

            var result = await _controller.ExportInventoryExcel(3, true, false);

            result.Should().BeOfType<FileContentResult>();
        }

        [Test]
        public async Task ExportAppointmentsPdf_ShouldReturnBadRequest_WhenExceptionThrown()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentsReportQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("PDF engine failure"));

            var result = await _controller.ExportAppointmentsPdf(null, null, null, null);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

      
        [Test]
        public async Task ExportInventoryPdf_ShouldReturnViewAsPdf_WhenSuccessful()
        {
            var data = new List<InventoryReportDto>();
            _mediatorMock.Setup(m => m.Send(
                    It.IsAny<GetInventoryReportQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);

           
            var result = await _controller.ExportInventoryPdf(null, null, null);

            
            result.Should().BeOfType<ViewAsPdf>();
            var pdfResult = result as ViewAsPdf;
            pdfResult!.FileName.Should().StartWith("Inventario_");
            pdfResult.FileName.Should().EndWith(".pdf");
        }
      
    }

}
