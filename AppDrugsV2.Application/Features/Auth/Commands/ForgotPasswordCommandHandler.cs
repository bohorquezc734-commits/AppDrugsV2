using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (user == null)
            {
                // Para no revelar si el correo existe o no, simulamos éxito
                return Result<bool>.Success(true);
            }

            // Generar código de 6 dígitos
            var random = new Random();
            var token = random.Next(100000, 999999).ToString();
            
            // Expiración en 15 minutos
            user.SetResetPasswordToken(token, DateTime.UtcNow.AddMinutes(15));
            
            await _context.SaveChangesAsync(cancellationToken);

            var subject = "Recuperación de contraseña";
            var body = $"Tu código de recuperación es: {token}\n\nEste código expira en 15 minutos.";
            
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Result<bool>.Success(true);
        }
    }
}
