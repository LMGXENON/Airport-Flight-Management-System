using AFMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AFMS.Controllers;

public class AccountController : Controller
{
    private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase) { "light", "dark" };
    private static readonly HashSet<string> AllowedAirports = new(StringComparer.OrdinalIgnoreCase) { "EGLL", "EGKK", "EGSS", "KJFK", "KLAX" };
    private static readonly HashSet<string> AllowedTimeFormats = new(StringComparer.OrdinalIgnoreCase) { "12", "24" };
    private static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "en" };

    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        // Simulate async for consistency (no real async work here)
        await Task.CompletedTask;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var configuredUsername = _configuration["Auth:AdminUsername"];
        var configuredPassword = _configuration["Auth:AdminPassword"];

        if (!string.Equals(model.Username, configuredUsername, StringComparison.Ordinal)
            || !string.Equals(model.Password, configuredPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var token = CreateToken(model.Username);
        Response.Cookies.Append("afms_auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(GetTokenExpiryHours()),
            Path = "/"
        });

        Response.Cookies.Append("afms_last_login_utc", DateTime.UtcNow.ToString("O"), new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(180),
            Path = "/"
        });

        var redirectUrl = ResolvePostLoginRedirect(model.ReturnUrl);
        await Task.CompletedTask;
        return LocalRedirect(redirectUrl);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("afms_auth_token");
        var loginUrl = Url.Action("Login", "Account");
        await Task.CompletedTask;
        return Redirect(loginUrl ?? "/Account/Login");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        var username = User.Identity?.Name ?? "Admin";
        var role = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value ?? "User";
        DateTime? lastLoginUtc = null;
        var lastLoginRaw = Request.Cookies["afms_last_login_utc"];
        if (!string.IsNullOrWhiteSpace(lastLoginRaw) && DateTime.TryParse(lastLoginRaw, out var parsedLastLogin))
        {
            lastLoginUtc = parsedLastLogin.ToUniversalTime();
        }

        var model = new AccountProfileViewModel
        {
            Username = username,
            Role = role,
            SessionExpiryHours = GetTokenExpiryHours(),
            LastLoginUtc = lastLoginUtc,
            LastUpdatedUtc = DateTime.UtcNow
        };

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Settings()
    {
        var model = new AccountSettingsViewModel
        {
            Theme = GetAllowedValue(Request.Cookies["afms_theme"], AllowedThemes, "light"),
            DefaultAirport = GetAllowedValue(
                Request.Cookies["afms_default_airport"],
                AllowedAirports,
                (_configuration["AeroDataBox:DefaultAirport"] ?? "EGLL").Trim().ToUpperInvariant()),
            TimeFormat = GetAllowedValue(Request.Cookies["afms_time_format"], AllowedTimeFormats, "24"),
            Language = GetAllowedValue(Request.Cookies["afms_language"], AllowedLanguages, "en"),
            Saved = TempData["SettingsSaved"] as bool? == true
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Settings(AccountSettingsViewModel model)
    {
        var normalizedTheme = GetAllowedValue(model.Theme, AllowedThemes, "light");
        var normalizedAirport = GetAllowedValue(
            model.DefaultAirport,
            AllowedAirports,
            (_configuration["AeroDataBox:DefaultAirport"] ?? "EGLL").Trim().ToUpperInvariant());
        var normalizedTimeFormat = GetAllowedValue(model.TimeFormat, AllowedTimeFormats, "24");
        var normalizedLanguage = GetAllowedValue(model.Language, AllowedLanguages, "en");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(180),
            Path = "/"
        };

        Response.Cookies.Append("afms_theme", normalizedTheme, cookieOptions);
        Response.Cookies.Append("afms_default_airport", normalizedAirport, cookieOptions);
        Response.Cookies.Append("afms_time_format", normalizedTimeFormat, cookieOptions);
        Response.Cookies.Append("afms_language", normalizedLanguage, cookieOptions);

        TempData["SettingsSaved"] = true;
        return RedirectToAction(nameof(Settings));
    }

    private string CreateToken(string username)
    {
        var issuer = _configuration["Auth:Issuer"] ?? "AFMS";
        var audience = _configuration["Auth:Audience"] ?? "AFMS.Users";
        var secret = _configuration["Auth:JwtSecret"]
            ?? throw new InvalidOperationException("Auth:JwtSecret must be configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryHours = GetTokenExpiryHours();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetTokenExpiryHours()
    {
        var configured = _configuration.GetValue<int?>("Auth:TokenExpiryHours") ?? 8;
        return Math.Clamp(configured, 1, 24);
    }

    private string ResolvePostLoginRedirect(string? returnUrl)
    {
        const string defaultUrl = "/Home/Index";

        if (string.IsNullOrWhiteSpace(returnUrl))
            return defaultUrl;

        var candidate = returnUrl.Trim();
        if (candidate.Length > 512)
            return defaultUrl;

        if (!Url.IsLocalUrl(candidate))
            return defaultUrl;

        if (candidate.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase))
            return defaultUrl;

        return candidate;
    }

    private static string GetAllowedValue(string? value, HashSet<string> allowedValues, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var normalized = value.Trim();
        return allowedValues.Contains(normalized) ? normalized : fallback;
    }
}

