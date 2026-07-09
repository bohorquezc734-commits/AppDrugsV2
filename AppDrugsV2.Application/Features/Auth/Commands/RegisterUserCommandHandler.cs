using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Domain.Enums;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar si el email ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
                return Result<int>.Failure("El email ya está registrado");

            // 2. Convertir el role de string a enum
            var role = request.Role?.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "pharmacist" => UserRole.Pharmacist,
                _ => UserRole.User
            };

            // 3. Hashear la contraseña
            var passwordHash = _passwordHasher.Hash(request.Password);

            // 4. Crear el usuario
            var user = new User(
                request.Email,
                passwordHash,
                request.FullName,
                role
            );

            // 5. Guardar en base de datos
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(user.Id);
        }
    }
}