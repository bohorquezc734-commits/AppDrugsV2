namespace AppDrugsV2.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(int userId, string email, string role);
    }
}