using Bogus;
using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services.MockServices;

public class MockUserService : IUserService
{
    private readonly List<UserDto> _users;

    public MockUserService()
    {
        var userFaker = new Faker<UserDto>("en")
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Username, f => f.Name.FirstName())
            .RuleFor(u => u.Role, f => f.PickRandom<UserRoles>());

        _users = userFaker.Generate(30);
    }

    public Task<List<UserDto>> GetListAsync()
    {
        return Task.FromResult(_users.ToList());
    }

    public Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user ?? new UserDto());
    }

    public Task<ApiResponseDto> CreateAsync(UserRegisterRequest request)
    {
        var newUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Role = UserRoles.User
        };
        _users.Add(newUser);

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "User successfully created (Mock)"
        });
    }

    public Task<ApiResponseDto> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser != null)
        {
            existingUser.Username = request.Username;
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "User successfully updated (Mock)"
        });
    }

    public Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "User successfully deleted (Mock)"
        });
    }
}
