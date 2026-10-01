using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Infrastructure;
using AppDrugsV2.Infrastructure.Persistence.Interceptors;
using AppDrugsV2.Infrastructure.Persistence;
using AppDrugsV2.Infrastructure.Services;
using AppDrugsV2.Infrastructure.Services.Auth;
using AppDrugsV2.Infrastructure.Services.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace AppDrugsV2.UnitTests.Infrastructure
{
    [TestFixture]
    public class DependencyInjectionTests
    {
        [Test]
        public void AddInfrastructure_ShouldRegisterServices()
        {
           
            var services = new ServiceCollection();
           
 
            var configurationMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(a => a.Value).Returns("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
            configurationMock.Setup(c => c.GetSection("ConnectionStrings:DefaultConnection")).Returns(configSectionMock.Object);
            
          
            services.AddInfrastructure(configurationMock.Object);

           
            Assert.That(services.Any(sd => sd.ServiceType == typeof(AuditableEntityInterceptor)), Is.True, "AuditableEntityInterceptor not registered.");
            Assert.That(services.Any(sd => sd.ServiceType == typeof(ApplicationDbContext)), Is.True, "ApplicationDbContext not registered.");
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IApplicationDbContext)), Is.True, "IApplicationDbContext not registered.");

          
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IPasswordHasher) && sd.ImplementationType == typeof(PasswordHasher)), Is.True);
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IJwtTokenGenerator) && sd.ImplementationType == typeof(JwtTokenGenerator)), Is.True);
            Assert.That(services.Any(sd => sd.ServiceType == typeof(ICurrentUserService) && sd.ImplementationType == typeof(CurrentUserService)), Is.True);

     
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IExcelExportService) && sd.ImplementationType == typeof(ExcelExportService)), Is.True);
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IEmailService) && sd.ImplementationType == typeof(EmailService)), Is.True);

          
            Assert.That(services.Any(sd => sd.ServiceType == typeof(IQrCodeService) && sd.ImplementationType == typeof(QrCodeService)), Is.True);
            Assert.That(services.Any(sd => sd.ServiceType == typeof(INotificationHubService) && sd.ImplementationType == typeof(NotificationHubService)), Is.True);
        }
    }
}
