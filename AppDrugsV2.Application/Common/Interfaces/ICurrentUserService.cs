namespace AppDrugsV2.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string Email { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
    }
}