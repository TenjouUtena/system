using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;
using SystemGame.Api.Services;

namespace SystemGame.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly RedisService _redisService;

    public AuthController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        JwtService jwtService,
        RedisService redisService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _redisService = redisService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User already exists" });
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Registration failed", errors = result.Errors });
        }

        // Add default role if needed
        await _userManager.AddToRoleAsync(user, "Player");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in Redis
        await _redisService.SetStringAsync(
            $"refresh_token:{user.Id}",
            refreshToken,
            TimeSpan.FromDays(30)
        );

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Roles = roles
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in Redis
        await _redisService.SetStringAsync(
            $"refresh_token:{user.Id}",
            refreshToken,
            TimeSpan.FromDays(30)
        );

        var response = new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Roles = roles
        };

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken) || string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest(new { message = "Invalid request" });
        }

        var storedRefreshToken = await _redisService.GetStringAsync($"refresh_token:{request.UserId}");
        if (storedRefreshToken != request.RefreshToken)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Update refresh token in Redis
        await _redisService.SetStringAsync(
            $"refresh_token:{request.UserId}",
            newRefreshToken,
            TimeSpan.FromDays(30)
        );

        var response = new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Roles = roles
        };

        return Ok(response);
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _redisService.KeyDeleteAsync($"refresh_token:{userId}");
        }

        return Ok(new { message = "Logged out successfully" });
    }
}

