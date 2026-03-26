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
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(LoginViewModel model)
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

        var redirectUrl = ResolvePostLoginRedirect(model.ReturnUrl);
        return LocalRedirect(redirectUrl);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("afms_auth_token");
        var loginUrl = Url.Action("Login", "Account");
        return Redirect(loginUrl ?? "/Account/Login");
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
}

