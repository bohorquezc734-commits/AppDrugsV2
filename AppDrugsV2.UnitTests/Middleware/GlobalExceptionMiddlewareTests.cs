using AppDrugsV2.Api.Middleware;
using AppDrugsV2.Application.Common.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Middleware
{
    [TestFixture]
    public class GlobalExceptionMiddlewareTests
    {
        private Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        }

        private static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<string> ReadResponseBodyAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }

        [Test]
        public async Task InvokeAsync_ShouldCallNext_WhenNoExceptionThrown()
        {
            var context = CreateHttpContext();
            var nextCalled = false;
            RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue();
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn404_WhenKeyNotFoundException()
        {
            var context = CreateHttpContext();
            RequestDelegate next = _ => throw new KeyNotFoundException("missing");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(404);
            var body = await ReadResponseBodyAsync(context);
            body.Should().Contain(AppConstants.Middleware.NotFoundError);
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn401_WhenUnauthorizedAccessException()
        {
            
            var context = CreateHttpContext();
            RequestDelegate next = _ => throw new UnauthorizedAccessException();
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(401);

            var body = await ReadResponseBodyAsync(context);

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(401);
            root.GetProperty("error").GetString()
                .Should().Contain(AppConstants.Middleware.UnauthorizedError);
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn400_WhenArgumentException()
        {
            var context = CreateHttpContext();
            var argumentMessage = "Invalid argument";
            RequestDelegate next = _ => throw new ArgumentException(argumentMessage);
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(400);
            var body = await ReadResponseBodyAsync(context);
            body.Should().Contain(argumentMessage);
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenUnhandledException()
        {
            
            var context = CreateHttpContext();
            RequestDelegate next = _ => throw new InvalidOperationException("boom");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            
            await middleware.InvokeAsync(context);

            
            context.Response.StatusCode.Should().Be(500);

            var body = await ReadResponseBodyAsync(context);

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(500);
            root.GetProperty("error").GetString()
                .Should().Contain(AppConstants.Middleware.InternalServerError);
        }

        [Test]
        public async Task InvokeAsync_ShouldSetContentTypeJson_WhenExceptionThrown()
        {
            var context = CreateHttpContext();
            RequestDelegate next = _ => throw new Exception("any");
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            context.Response.ContentType.Should().Be(AppConstants.Middleware.ContentTypeJson);
        }

        [Test]
        public async Task InvokeAsync_ShouldSerializeTraceId_InResponse()
        {
            var context = CreateHttpContext();
            context.TraceIdentifier = "test-trace-123";
            RequestDelegate next = _ => throw new KeyNotFoundException();
            var middleware = new GlobalExceptionMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            var body = await ReadResponseBodyAsync(context);
            body.Should().Contain("test-trace-123");
        }
    }
}
