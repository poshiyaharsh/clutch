using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Database & Identity (supports Railway DATABASE_URL URI or standard Npgsql connection string)
var rawConn = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? string.Empty;
var connectionString = NormalizePostgresConnectionString(rawConn);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

static string NormalizePostgresConnectionString(string input)
{
    if (string.IsNullOrWhiteSpace(input) ||
        (!input.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
         !input.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
    {
        return input;
    }

    var uri = new Uri(input);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');
    var port = uri.Port > 0 ? uri.Port : 5432;

    return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("RequireOrganizer", policy => policy.RequireRole(Roles.Admin, Roles.Organizer));
    options.AddPolicy("RequireJudge", policy => policy.RequireRole(Roles.Admin, Roles.Judge));
    options.AddPolicy("RequireParticipant", policy => policy.RequireRole(Roles.Participant));
});

// Domain Services
builder.Services.AddScoped<IHackathonService, HackathonService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddSignalR();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 15;
        opt.QueueLimit = 0;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Explicit CLI migration and seeding commands (never executed automatically at startup)
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    return;
}
if (args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
{
    await DbSeeder.SeedDemoDataAsync(app.Services, app.Configuration);
    return;
}
if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    await DbSeeder.SeedRolesAndAdminAsync(app.Services, app.Configuration);
    return;
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Content(
    System.Text.Json.JsonSerializer.Serialize(new
    {
        status = "healthy",
        service = "clutch",
        utc = DateTime.UtcNow
    }),
    "application/json",
    statusCode: StatusCodes.Status200OK)).AllowAnonymous();

app.MapGet("/health/ready", async (AppDbContext db, CancellationToken ct) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        return canConnect
            ? Results.Content(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = "ready",
                    database = "connected",
                    utc = DateTime.UtcNow
                }),
                "application/json",
                statusCode: StatusCodes.Status200OK)
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

app.MapHub<Clutch.Web.Hubs.LeaderboardHub>("/hubs/leaderboard");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
