using AFMS.Models;
using AFMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AFMS.Controllers;

public class AccountController : Controller
{
    private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase) { "light", "dark" };
    private static readonly HashSet<string> AllowedAirports = new(StringComparer.OrdinalIgnoreCase) { "EGLL", "EGKK", "EGSS", "EGGW", "EGLC", "KJFK", "KLAX" };
    private static readonly HashSet<string> AllowedTimeFormats = new(StringComparer.OrdinalIgnoreCase) { "12", "24" };
    private static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "en" };

    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public AccountController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
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

        var credentialCheck = await ValidateCredentialsAsync(model.Username, model.Password);
        if (!credentialCheck.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var token = CreateToken(credentialCheck.CanonicalUsername);
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

        _context.LoginAuditEntries.Add(new LoginAuditEntry
        {
            Username = credentialCheck.CanonicalUsername,
            IpAddress = GetRequestIpAddress(),
            OccurredUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

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
    public async Task<IActionResult> Profile()
    {
        var username = User.Identity?.Name ?? "Admin";
        var role = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value ?? "User";

        DateTime? lastLoginUtc = null;
        var lastLoginRaw = Request.Cookies["afms_last_login_utc"];
        if (!string.IsNullOrWhiteSpace(lastLoginRaw) && DateTime.TryParse(lastLoginRaw, out var parsedLastLogin))
        {
            lastLoginUtc = parsedLastLogin.ToUniversalTime();
        }

        DateTime? sessionExpiresUtc = null;
        var sessionRemainingSeconds = 0;
        var authToken = Request.Cookies["afms_auth_token"];
        if (!string.IsNullOrWhiteSpace(authToken))
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(authToken);
                if (jwt.ValidTo > DateTime.MinValue)
                {
                    sessionExpiresUtc = jwt.ValidTo;
                    sessionRemainingSeconds = Math.Max(0, (int)(jwt.ValidTo - DateTime.UtcNow).TotalSeconds);
                }
            }
            catch
            {
                sessionRemainingSeconds = 0;
            }
        }

        var loginHistory = await _context.LoginAuditEntries
            .AsNoTracking()
            .Where(entry => entry.Username == username)
            .OrderByDescending(entry => entry.OccurredUtc)
            .Take(5)
            .Select(entry => new LoginHistoryItem
            {
                OccurredUtc = entry.OccurredUtc,
                IpAddress = entry.IpAddress
            })
            .ToListAsync();

        var model = new AccountProfileViewModel
        {
            Username = username,
            Role = role,
            SessionExpiryHours = GetTokenExpiryHours(),
            SessionExpiresUtc = sessionExpiresUtc,
            SessionRemainingSeconds = sessionRemainingSeconds,
            LastLoginUtc = lastLoginUtc,
            LastUpdatedUtc = DateTime.UtcNow,
            LoginHistory = loginHistory,
            PasswordChanged = TempData["PasswordChanged"] as bool? == true,
            PasswordChangeError = TempData["PasswordChangeError"] as string
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PasswordChangeError"] = "Please fix the password form errors and try again.";
            return RedirectToAction(nameof(Profile));
        }

        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            TempData["PasswordChangeError"] = "Unable to determine current user.";
            return RedirectToAction(nameof(Profile));
        }

        var currentCheck = await ValidateCredentialsAsync(username, model.CurrentPassword);
        if (!currentCheck.IsValid)
        {
            TempData["PasswordChangeError"] = "Current password is incorrect.";
            return RedirectToAction(nameof(Profile));
        }

        if (string.Equals(model.CurrentPassword, model.NewPassword, StringComparison.Ordinal))
        {
            TempData["PasswordChangeError"] = "New password must be different from the current password.";
            return RedirectToAction(nameof(Profile));
        }

        var existingCredential = await _context.AuthCredentials
            .FirstOrDefaultAsync(c => c.Username == currentCheck.CanonicalUsername);

        var newHash = _passwordHasher.HashPassword(currentCheck.CanonicalUsername, model.NewPassword);
        if (existingCredential is null)
        {
            _context.AuthCredentials.Add(new AuthCredential
            {
                Username = currentCheck.CanonicalUsername,
                PasswordHash = newHash,
                UpdatedUtc = DateTime.UtcNow
            });
        }
        else
        {
            existingCredential.PasswordHash = newHash;
            existingCredential.UpdatedUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["PasswordChanged"] = true;
        return RedirectToAction(nameof(Profile));
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

    private async Task<(bool IsValid, string CanonicalUsername)> ValidateCredentialsAsync(string username, string password)
    {
        var configuredUsername = (_configuration["Auth:AdminUsername"] ?? string.Empty).Trim();
        var configuredPassword = _configuration["Auth:AdminPassword"] ?? string.Empty;
        var requestedUsername = username.Trim();
        var normalizedConfiguredUsername = configuredUsername.ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(configuredUsername))
        {
            return (false, string.Empty);
        }

        if (!string.Equals(requestedUsername, configuredUsername, StringComparison.OrdinalIgnoreCase))
        {
            return (false, configuredUsername);
        }

        var storedCredential = await _context.AuthCredentials
            .FirstOrDefaultAsync(c => c.Username.ToUpper() == normalizedConfiguredUsername);

        if (storedCredential is not null)
        {
            var hashUsername = string.IsNullOrWhiteSpace(storedCredential.Username)
                ? configuredUsername
                : storedCredential.Username;

            var verificationCandidates = new[]
            {
                hashUsername,
                configuredUsername,
                requestedUsername
            }
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToList();

            var hashMatched = false;
            var needsRehash = false;
            foreach (var candidate in verificationCandidates)
            {
                var result = _passwordHasher.VerifyHashedPassword(candidate, storedCredential.PasswordHash, password);
                if (result != PasswordVerificationResult.Failed)
                {
                    hashMatched = true;
                    needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded
                        || !string.Equals(storedCredential.Username, configuredUsername, StringComparison.Ordinal);
                    break;
                }
            }

            if (hashMatched)
            {
                if (needsRehash)
                {
                    storedCredential.Username = configuredUsername;
                    storedCredential.PasswordHash = _passwordHasher.HashPassword(configuredUsername, password);
                    storedCredential.UpdatedUtc = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return (true, configuredUsername);
            }

            // Legacy recovery path: if an old row stored plain text instead of a hash,
            // accept once and migrate to a proper hash.
            if (string.Equals(storedCredential.PasswordHash, password, StringComparison.Ordinal))
            {
                storedCredential.Username = configuredUsername;
                storedCredential.PasswordHash = _passwordHasher.HashPassword(configuredUsername, password);
                storedCredential.UpdatedUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (true, configuredUsername);
            }

            // Recovery path for environments where AUTH_ADMIN_PASSWORD has been rotated
            // but an older hashed credential still exists in the database.
            if (!string.IsNullOrWhiteSpace(configuredPassword)
                && string.Equals(password, configuredPassword, StringComparison.Ordinal))
            {
                storedCredential.PasswordHash = _passwordHasher.HashPassword(configuredUsername, configuredPassword);
                storedCredential.UpdatedUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (true, configuredUsername);
            }

            return (false, configuredUsername);
        }

        return (string.Equals(password, configuredPassword, StringComparison.Ordinal), configuredUsername);
    }

    private string GetRequestIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var first = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(first))
                return first;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

