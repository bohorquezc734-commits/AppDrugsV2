using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    /// <summary>
    /// Comando para renovar el Access Token usando un Refresh Token válido.
    /// </summary>
    public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar al usuario que tenga ese Refresh Token
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null)
                return Result<RefreshTokenResponse>.Failure("Refresh Token inválido.");

            // 2. Verificar que el Refresh Token no haya caducado
            if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return Result<RefreshTokenResponse>.Failure("El Refresh Token ha expirado. Por favor, inicia sesión nuevamente.");

            // 3. Verificar que el usuario siga activo
            if (!user.IsActive)
                return Result<RefreshTokenResponse>.Failure("El usuario fue desactivado.");

            // 4. Generar nuevo Access Token
            var newToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString());

            // 5. Rotar el Refresh Token (cada uso genera uno nuevo — mayor seguridad)
            var newRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.SetRefreshToken(newRefreshToken, newRefreshTokenExpiry);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse
            {
                Token                 = newToken,
                ExpiresAt             = DateTime.UtcNow.AddHours(1),
                RefreshToken          = newRefreshToken,
                RefreshTokenExpiresAt = newRefreshTokenExpiry
            });
        }
    }
}
