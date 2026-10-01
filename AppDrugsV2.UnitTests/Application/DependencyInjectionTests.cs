using AppDrugsV2.Application;
using AppDrugsV2.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;

namespace AppDrugsV2.UnitTests.Application
{
    [TestFixture]
    public class DependencyInjectionTests
    {
        [Test]
        public void AddApplication_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert
            // 1. Verify MediatR registration
            var mediatRRegistered = services.Any(sd => sd.ServiceType == typeof(IMediator));
            Assert.That(mediatRRegistered, Is.True, "MediatR was not registered.");

            // 2. Verify ValidationBehavior registration
            var validationBehaviorRegistered = services.Any(sd => 
                sd.ServiceType == typeof(IPipelineBehavior<,>) && 
                sd.ImplementationType == typeof(ValidationBehavior<,>));
            Assert.That(validationBehaviorRegistered, Is.True, "ValidationBehavior was not registered.");

            var behaviorLifetime = services.FirstOrDefault(sd => 
                sd.ServiceType == typeof(IPipelineBehavior<,>) && 
                sd.ImplementationType == typeof(ValidationBehavior<,>))?.Lifetime;
            Assert.That(behaviorLifetime, Is.EqualTo(ServiceLifetime.Scoped));
        }
    }
}
