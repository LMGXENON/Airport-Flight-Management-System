using AFMS.BackgroundServices;
using AFMS.Data;
using AFMS.Hubs;
using AFMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

AddDotEnvConfiguration(builder);

// Add services to the container.
builder.Services.AddControllersWithViews();

var jwtSecret = builder.Configuration["Auth:JwtSecret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("Auth:JwtSecret must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = builder.Configuration["Auth:Issuer"],
            ValidAudience = builder.Configuration["Auth:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("afms_auth_token", out var token)
                    && !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (context.Request.Path.StartsWithSegments("/Account/Login"))
                {
                    return Task.CompletedTask;
                }

                context.HandleResponse();
                context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(context.Request.Path + context.Request.QueryString)}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add memory cache to avoid API rate limits
builder.Services.AddMemoryCache();

// Add HttpClient for API calls
builder.Services.AddHttpClient<AeroDataBoxService>();
builder.Services.AddHttpClient("DeepSeek", client =>
{
    var apiEndpoint = builder.Configuration["DeepSeek:ApiEndpoint"] ?? "https://api.deepseek.com/v1/";
    client.BaseAddress = new Uri($"{apiEndpoint.TrimEnd('/')}/");
});

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use PostgreSQL if connection string contains "Host=" (PostgreSQL), otherwise use SQLite
    if (connectionString?.Contains("Host=") == true)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Add flight sync service
builder.Services.AddScoped<FlightSyncService>();

// Add advanced-search service
builder.Services.AddScoped<FlightSearchService>();
builder.Services.AddScoped<ManualFlightMergeService>();

// Add background service for periodic flight updates
builder.Services.AddHostedService<FlightUpdateBackgroundService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    InitializeDatabaseWithRetry(dbContext, startupLogger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

// Serve static assets (CSS/JS/images) before auth so the login page can be styled.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// Map SignalR hub
app.MapHub<FlightHub>("/flightHub");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static void AddDotEnvConfiguration(WebApplicationBuilder builder)
{
    var configValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    var envFilePaths = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"))
    }.Distinct(StringComparer.OrdinalIgnoreCase);

    foreach (var envFilePath in envFilePaths)
    {
        if (!File.Exists(envFilePath))
            continue;

        foreach (var entry in ParseDotEnvFile(envFilePath))
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(entry.Key)))
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);

            foreach (var configKey in GetConfigurationKeys(entry.Key))
                configValues[configKey] = entry.Value;
        }
    }

    // Map environment variables that were set externally (e.g. by Docker env_file)
    // even when no .env file exists on disk inside the container.
    string[] knownEnvVars =
    [
        "AERODATABOX_API_KEY", "AERODATABOX_API_HOST", "DEFAULT_AIRPORT",
        "DEEPSEEK_API_KEY", "DEEPSEEK_API_ENDPOINT", "DEEPSEEK_MODEL",
        "DEEPSEEK_TIMEOUT_SECONDS", "DEEPSEEK_MAX_REQUESTS_PER_MINUTE", "DEEPSEEK_PROMPT_FILE",
        "AUTH_ADMIN_USERNAME", "AUTH_ADMIN_PASSWORD", "AUTH_JWT_SECRET", "AUTH_ISSUER", "AUTH_AUDIENCE", "AUTH_TOKEN_EXPIRY_HOURS"
    ];

    foreach (var envVar in knownEnvVars)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value))
        {
            foreach (var configKey in GetConfigurationKeys(envVar))
                configValues.TryAdd(configKey, value);
        }
    }

    if (configValues.Count > 0)
        builder.Configuration.AddInMemoryCollection(configValues);
}

static IEnumerable<KeyValuePair<string, string?>> ParseDotEnvFile(string filePath)
{
    foreach (var rawLine in File.ReadAllLines(filePath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            continue;

        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            continue;

        var value = parts[1].Trim();
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            value = value[1..^1];

        yield return new KeyValuePair<string, string?>(parts[0], value);
    }
}

static IEnumerable<string> GetConfigurationKeys(string key)
{
    yield return key.Replace("__", ":", StringComparison.Ordinal);

    var alias = GetDotEnvAlias(key);
    if (!string.IsNullOrWhiteSpace(alias))
        yield return alias;
}

static string? GetDotEnvAlias(string key) => key switch
{
    "AERODATABOX_API_KEY" => "AeroDataBox:ApiKey",
    "AERODATABOX_API_HOST" => "AeroDataBox:ApiHost",
    "DEFAULT_AIRPORT" => "AeroDataBox:DefaultAirport",
    "DEEPSEEK_API_KEY" => "DeepSeek:ApiKey",
    "DEEPSEEK_API_ENDPOINT" => "DeepSeek:ApiEndpoint",
    "DEEPSEEK_MODEL" => "DeepSeek:Model",
    "DEEPSEEK_TIMEOUT_SECONDS" => "DeepSeek:TimeoutSeconds",
    "DEEPSEEK_MAX_REQUESTS_PER_MINUTE" => "DeepSeek:MaxRequestsPerMinute",
    "DEEPSEEK_PROMPT_FILE" => "DeepSeek:PromptFile",
    "AUTH_ADMIN_USERNAME" => "Auth:AdminUsername",
    "AUTH_ADMIN_PASSWORD" => "Auth:AdminPassword",
    "AUTH_JWT_SECRET" => "Auth:JwtSecret",
    "AUTH_ISSUER" => "Auth:Issuer",
    "AUTH_AUDIENCE" => "Auth:Audience",
    "AUTH_TOKEN_EXPIRY_HOURS" => "Auth:TokenExpiryHours",
    _ => null
};

static void ApplyStartupSchemaUpdates(ApplicationDbContext dbContext, ILogger startupLogger)
{
    if (!dbContext.Database.IsSqlite())
    {
        startupLogger.LogInformation("Skipping SQLite-specific startup schema updates for provider {ProviderName}.", dbContext.Database.ProviderName);
        return;
    }

    if (dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
    {
        dbContext.Database.OpenConnection();
    }

    try
    {
        if (!SqliteColumnExists(dbContext, "Flights", "IsManualEntry"))
        {
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE Flights ADD COLUMN IsManualEntry INTEGER NOT NULL DEFAULT 0");
            startupLogger.LogInformation("Added missing IsManualEntry column to Flights table.");
        }
        else
        {
            startupLogger.LogDebug("IsManualEntry column already exists; skipping startup schema update.");
        }

        if (!SqliteColumnExists(dbContext, "Flights", "AircraftType"))
        {
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE Flights ADD COLUMN AircraftType TEXT NULL");
            startupLogger.LogInformation("Added missing AircraftType column to Flights table.");
        }
        else
        {
            startupLogger.LogDebug("AircraftType column already exists; skipping startup schema update.");
        }

        if (!SqliteIndexExists(dbContext, "IX_Flights_FlightNumber_DepartureTime"))
        {
            dbContext.Database.ExecuteSqlRaw("CREATE INDEX IX_Flights_FlightNumber_DepartureTime ON Flights (FlightNumber, DepartureTime)");
            startupLogger.LogInformation("Added composite index IX_Flights_FlightNumber_DepartureTime to Flights table.");
        }
        else
        {
            startupLogger.LogDebug("IX_Flights_FlightNumber_DepartureTime already exists; skipping startup schema update.");
        }
    }
    finally
    {
        dbContext.Database.CloseConnection();
    }
}

static void InitializeDatabaseWithRetry(ApplicationDbContext dbContext, ILogger startupLogger)
{
    const int maxAttempts = 5;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            dbContext.Database.EnsureCreated();
            ApplyStartupSchemaUpdates(dbContext, startupLogger);
            startupLogger.LogInformation("Database initialized successfully.");
            return;
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "Database initialization failed on attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);

            if (attempt == maxAttempts)
            {
                startupLogger.LogWarning("Continuing startup without confirmed database initialization to keep container alive.");
                return;
            }

            Thread.Sleep(TimeSpan.FromSeconds(attempt * 3));
        }
    }
}

static bool SqliteColumnExists(ApplicationDbContext dbContext, string tableName, string columnName)
{
    var connection = dbContext.Database.GetDbConnection();
    using var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA table_info('{tableName.Replace("'", "''")}')";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        var existingColumnName = reader.GetString(1);
        if (string.Equals(existingColumnName, columnName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static bool SqliteIndexExists(ApplicationDbContext dbContext, string indexName)
{
    var connection = dbContext.Database.GetDbConnection();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'index' AND name = $name LIMIT 1";

    var parameter = command.CreateParameter();
    parameter.ParameterName = "$name";
    parameter.Value = indexName;
    command.Parameters.Add(parameter);

    using var reader = command.ExecuteReader();
    return reader.Read();
}