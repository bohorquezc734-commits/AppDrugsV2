using AppDrugsV2.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Common.Behaviors
{
    [TestFixture]
    public class ValidationBehaviorTests
    {
        public class TestRequest : IRequest<TestResponse> { }
        public class TestResponse { }

        [Test]
        public async Task Handle_WithNoValidators_ShouldInvokeNextAndReturnResponse()
        {

            var validators = new List<IValidator<TestRequest>>();
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
            
            var request = new TestRequest();
            var expectedResponse = new TestResponse();
            RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(expectedResponse);


            var response = await behavior.Handle(request, next, CancellationToken.None);

   
            response.Should().Be(expectedResponse);
        }

        [Test]
        public async Task Handle_WithValidatorsButNoErrors_ShouldInvokeNextAndReturnResponse()
        {

            var validatorMock = new Mock<IValidator<TestRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());
                         
            var validators = new List<IValidator<TestRequest>> { validatorMock.Object };
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
            
            var request = new TestRequest();
            var expectedResponse = new TestResponse();
            RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(expectedResponse);

 
            var response = await behavior.Handle(request, next, CancellationToken.None);

        
            response.Should().Be(expectedResponse);
            validatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_WithValidationErrors_ShouldThrowValidationExceptionWithErrorMessages()
        {
      
            var validatorMock1 = new Mock<IValidator<TestRequest>>();
            validatorMock1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult(new List<ValidationFailure> 
                          { 
                              new ValidationFailure("Property1", "Error message 1") 
                          }));

            var validatorMock2 = new Mock<IValidator<TestRequest>>();
            validatorMock2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult(new List<ValidationFailure> 
                          { 
                              new ValidationFailure("Property2", "Error message 2") 
                          }));

            var validators = new List<IValidator<TestRequest>> { validatorMock1.Object, validatorMock2.Object };
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
            
            var request = new TestRequest();
            RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(new TestResponse());

          
            var exception = Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(request, next, CancellationToken.None));
            
            
            exception.Message.Should().Contain("Error message 1");
            exception.Message.Should().Contain("Error message 2");
        }
    }
}
