using AFMS.BackgroundServices;
using AFMS.Data;
using AFMS.Hubs;
using AFMS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
var envFile = Path.Combine(builder.Environment.ContentRootPath, "..", ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
    }
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add memory cache to avoid API rate limits
builder.Services.AddMemoryCache();

// Add HttpClient for API calls
builder.Services.AddHttpClient<AeroDataBoxService>();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add flight sync service
builder.Services.AddScoped<FlightSyncService>();

// Add advanced-search service
builder.Services.AddScoped<FlightSearchService>();

// Add background service for periodic flight updates
builder.Services.AddHostedService<FlightUpdateBackgroundService>();

var app = builder.Build();

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Map SignalR hub
app.MapHub<FlightHub>("/flightHub");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();