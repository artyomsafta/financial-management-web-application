using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return null;
        }

        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(UserRegisterRequest request)
    {
        await _context.Users.EnsureUsernameNotTakenAync(request.Username);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim()
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        try
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return MapToDto(newUser);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new user {Username}", request.Username);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        await _context.Users.EnsureUsernameNotTakenAync(request.Username);

        var user = await _context.Users.FindAsync(id);
        if (user is null) 
        { 
            return false;
        }

        user.Username = request.Username.Trim();
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        try
        { 
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating user {Username}", request.Username);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        if (user.Wallets.Any(w => !w.IsDeleted))
        {
            throw new InvalidOperationException("You cannot delete a user that has active wallets.");
        }

        try
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while soft deleting user {Username}", user.Username);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}
