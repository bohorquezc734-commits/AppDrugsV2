using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;

namespace AppDrugsV2.Application.Features.Auth.Queries
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<LoginResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginQueryHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<LoginResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            // 1. Buscar usuario por email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
                return Result<LoginResponse>.Failure("Credenciales inválidas");

            // 2. Verificar si está activo
            if (!user.IsActive)
                return Result<LoginResponse>.Failure("Usuario desactivado");

            // 3. Validar contraseña
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Credenciales inválidas");

            // 4. Registrar el login
            user.RecordLogin();
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Generar token JWT
            var token = _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.Email,
                user.Role.ToString()
            );

            // 6. Crear respuesta
            var response = new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return Result<LoginResponse>.Success(response);
        }
    }
}