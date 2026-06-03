namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IUserContext
{
    Guid UserId { get; }
    string Username { get; }
    bool IsAdmin { get; }
}
