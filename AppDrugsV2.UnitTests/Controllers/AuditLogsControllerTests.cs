using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.AuditLogs.Queries.GetAuditLogs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class AuditLogsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private AuditLogsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuditLogsController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetAuditLogs_ShouldReturnOkResult_WithPagedResult()
        {
            var query = new GetAuditLogsQuery { PageNumber = 1, PageSize = 10 };
            var pagedResult = new PagedResult<AuditLogDto>(new List<AuditLogDto>(), 0, 1, 10);
            
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

           
            var result = await _controller.GetAuditLogs(query);

            
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResult);
        }
    }
}
