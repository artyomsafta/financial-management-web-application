using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
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
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            throw new InvalidOperationException("A user with the same name already exists.");
        }

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
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            throw new InvalidOperationException("A user with the same name already exists.");
        }

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser is null) 
        { 
            return false;
        }

        try
        { 
            existingUser.Username = request.Username.Trim();
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, request.Password);
            await _context.SaveChangesAsync();

            return true;

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating user {Username}", request.Username);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users

            //TODO: include wallets of the user to check if there are any non-deleted wallets before allowing deletion
            //.Include(...)

            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        //TODO: check if there are any non-deleted wallets for the user and prevent deletion if there are any

        //if (...)
        //{
        //    throw new InvalidOperationException("You cannot delete a user that has active wallets.");
        //}

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while deleting user {Username}", user.Username);
            throw;
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
