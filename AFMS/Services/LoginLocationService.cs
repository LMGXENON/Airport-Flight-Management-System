using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace AFMS.Services;

public class LoginLocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LoginLocationService> _logger;

    public LoginLocationService(
        HttpClient httpClient,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<LoginLocationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> ResolveLocationAsync(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "Unknown")
            return "Unknown";

        if (ipAddress == "localhost")
            return "Localhost";

        if (!bool.TryParse(_configuration["IpGeolocation:Enabled"], out var enabled) || !enabled)
            return "Unavailable";

        if (IsPrivateOrReserved(ipAddress))
            return "Private network";

        var cacheKey = $"geoip:{ipAddress}";
        if (_cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
            return cached;

        var baseUrl = _configuration["IpGeolocation:ApiBaseUrl"] ?? "https://ipapi.co";
        var timeoutSeconds = _configuration.GetValue<int?>("IpGeolocation:TimeoutSeconds") ?? 3;
        timeoutSeconds = Math.Clamp(timeoutSeconds, 1, 10);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            var requestUrl = $"{baseUrl.TrimEnd('/')}/{ipAddress}/json/";
            using var response = await _httpClient.GetAsync(requestUrl, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("GeoIP lookup failed for {IpAddress} with status {StatusCode}", ipAddress, response.StatusCode);
                return Cache(cacheKey, "Unavailable", TimeSpan.FromMinutes(10));
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(content);

            var city = ReadString(doc.RootElement, "city");
            var countryName = ReadString(doc.RootElement, "country_name");
            var countryCode = ReadString(doc.RootElement, "country_code");

            string location;
            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(countryName))
                location = $"{city}, {countryName}";
            else if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(countryCode))
                location = $"{city}, {countryCode}";
            else if (!string.IsNullOrWhiteSpace(countryName))
                location = countryName;
            else if (!string.IsNullOrWhiteSpace(countryCode))
                location = countryCode;
            else
                location = "Unavailable";

            return Cache(cacheKey, location, TimeSpan.FromHours(6));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GeoIP lookup exception for {IpAddress}", ipAddress);
            return Cache(cacheKey, "Unavailable", TimeSpan.FromMinutes(10));
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
            return string.Empty;

        return value.GetString() ?? string.Empty;
    }

    private string Cache(string key, string value, TimeSpan duration)
    {
        _cache.Set(key, value, duration);
        return value;
    }

    private static bool IsPrivateOrReserved(string ipText)
    {
        if (!IPAddress.TryParse(ipText, out var ip))
            return true;

        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();
            // fc00::/7 unique local addresses
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;

            // fe80::/10 link-local addresses
            if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
                return true;

            return false;
        }

        var octets = ip.GetAddressBytes();
        if (octets.Length != 4)
            return true;

        if (octets[0] == 10)
            return true;

        if (octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31)
            return true;

        if (octets[0] == 192 && octets[1] == 168)
            return true;

        if (octets[0] == 127)
            return true;

        if (octets[0] == 169 && octets[1] == 254)
            return true;

        return false;
    }
}
