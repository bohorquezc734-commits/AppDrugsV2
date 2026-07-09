using FluentValidation;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class CreateDrugCommandValidator : AbstractValidator<CreateDrugCommand>
    {
        public CreateDrugCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nombre es requerido")
                .MaximumLength(100).WithMessage("Nombre no debe exceder 100 caracteres");

            RuleFor(x => x.GenericName)
                .NotEmpty().WithMessage("Nombre genérico es requerido")
                .MaximumLength(100).WithMessage("Nombre genérico no debe exceder 100 caracteres");

            RuleFor(x => x.Laboratory)
                .NotEmpty().WithMessage("Laboratorio es requerido")
                .MaximumLength(100).WithMessage("Laboratorio no debe exceder 100 caracteres");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("El precio debe ser mayor que 0");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Categoría es requerida")
                .MaximumLength(50).WithMessage("Categoría no debe exceder 50 caracteres");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de expiración debe ser futura");
        }
    }
}