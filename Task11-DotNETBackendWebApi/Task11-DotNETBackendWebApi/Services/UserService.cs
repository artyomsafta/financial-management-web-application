using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<UserService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(
        AppDbContext context, 
        IUserContext userContext, 
        ILogger<UserService> logger
    )
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<UserDto>> GetListAsync()
    {
        var query = _context.Users.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(u => u.Id == _userContext.UserId);
        }

        return await query
            .AsNoTracking()
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        return user.MapToUserDto();
    }

    public async Task<Guid> CreateAsync(UserRegisterRequest request)
    {
        if (await _context.Users.AnyAsync(t => t.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            throw new InvalidOperationException("A user with the same username already exists.");
        }

        var passwordErrorMessage = ValidatePassword(request.Password);
        if (!string.IsNullOrEmpty(passwordErrorMessage))
        {
            throw new ValidationException(passwordErrorMessage);
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

            return newUser.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating a new user {Username}", request.Username);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null) 
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (await _context.Users.AnyAsync(t => t.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            throw new InvalidOperationException("A user with the same username already exists.");
        }

        var passwordErrorMessage = ValidatePassword(request.Password);
        if (!string.IsNullOrEmpty(passwordErrorMessage))
        {
            throw new ValidationException(passwordErrorMessage);
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
            _logger.LogError(ex, "Database error occurred while updating user {Username}", request.Username);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(UpdateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
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
            _logger.LogError(ex, "Database error occurred while deleting user {Username}", user.Username);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    private string ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password cannot be empty or consist only of spaces.";
        }

        if (password.Length < 8 || password.Length > 64)
        {
            return "Password must be between 8 and 64 characters long.";
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return "Password must contain at least one uppercase letter.";

        if (!Regex.IsMatch(password, @"[0-9]"))
            return "Password must contain at least one digit.";

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
            return "Password must contain at least one special character.";

        return "";
    }
}
