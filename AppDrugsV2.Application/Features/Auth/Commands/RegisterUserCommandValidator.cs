using FluentValidation;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(AppConstants.Messages.EmailRequired)
                .EmailAddress().WithMessage(AppConstants.Messages.EmailInvalidFormat)
                .MaximumLength(AppConstants.Validation.EmailMaxLength).WithMessage(AppConstants.Messages.EmailMaxLengthMsg);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(AppConstants.Messages.PasswordRequired)
                .MinimumLength(AppConstants.Validation.PasswordMinLength).WithMessage(AppConstants.Messages.PasswordMinLengthMsg)
                .Matches(AppConstants.Validation.RegexUpperCase).WithMessage(AppConstants.Messages.PasswordUpperCase)
                .Matches(AppConstants.Validation.RegexLowerCase).WithMessage(AppConstants.Messages.PasswordLowerCase)
                .Matches(AppConstants.Validation.RegexDigit).WithMessage(AppConstants.Messages.PasswordDigit);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(AppConstants.Messages.FullNameRequired)
                .MaximumLength(AppConstants.Validation.FullNameMaxLength).WithMessage(AppConstants.Messages.FullNameMaxLengthMsg);

            RuleFor(x => x.Role)
                .Must(r => r == AppConstants.Roles.User || r == AppConstants.Roles.Pharmacist || r == AppConstants.Roles.Admin)
                .WithMessage(AppConstants.Messages.RoleInvalid);
        }
    }
}