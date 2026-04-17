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
    private readonly IUserContext _userContext;
    private readonly ILogger<UserService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IUserContext userContext, ILogger<UserService> logger)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<UserDto>> GetListAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> CreateAsync(UserRegisterRequest request)
    {
        if (await _context.Users.AnyAsync(t => t.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            return Result<UserDto>.Failure("A user with the same username already exists.");
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

            return Result<UserDto>.Success(MapToDto(newUser));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new user {Username}", request.Username);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null) 
        { 
            return Result.Failure($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (await _context.Users.AnyAsync(t => t.Username.ToLower() == request.Username.Trim().ToLower()))
        {
            return Result.Failure("A user with the same username already exists.");
        }

        user.Username = request.Username.Trim();
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        try
        { 
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating user {Username}", request.Username);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return Result.Failure($"User with ID {id} not found");
        }

        if (!_userContext.IsAdmin && user.Id != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (user.Wallets.Any(w => !w.IsDeleted))
        {
            return Result.Failure("You cannot delete a user that has active wallets.");
        }

        try
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Success();
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
