using FluentValidation;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email es requerido")
                .EmailAddress().WithMessage("Formato de email inválido")
                .MaximumLength(100).WithMessage("Email no debe exceder 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password es requerido")
                .MinimumLength(8).WithMessage("Password debe tener al menos 8 caracteres")
                .Matches("[A-Z]").WithMessage("Password debe tener al menos una mayúscula")
                .Matches("[a-z]").WithMessage("Password debe tener al menos una minúscula")
                .Matches("[0-9]").WithMessage("Password debe tener al menos un número");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Nombre completo es requerido")
                .MaximumLength(100).WithMessage("Nombre no debe exceder 100 caracteres");

            RuleFor(x => x.Role)
                .Must(r => r == "User" || r == "Pharmacist" || r == "Admin")
                .WithMessage("Rol inválido. Debe ser User, Pharmacist o Admin");
        }
    }
}