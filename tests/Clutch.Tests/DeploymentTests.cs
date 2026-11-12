using Clutch.Web.Data;
using Clutch.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Clutch.Tests;

public class DeploymentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeploymentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private ServiceProvider BuildIsolatedServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOkWithHealthyPayload()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: $"Response body: {body}");
        body.Should().Contain("\"status\":\"healthy\"");
        body.Should().Contain("\"service\":\"clutch\"");
    }

    [Fact]
    public async Task DbSeeder_ThrowsWhenAdminPasswordNotConfigured()
    {
        // Arrange
        using var sp = BuildIsolatedServiceProvider(nameof(DbSeeder_ThrowsWhenAdminPasswordNotConfigured));
        var config = new ConfigurationBuilder().Build();

        // Act
        Func<Task> act = () => DbSeeder.SeedRolesAndAdminAsync(sp, config);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SeedAdmin:Password*");
    }

    [Fact]
    public async Task DbSeeder_SeedsRolesAndAdminIdempotently()
    {
        // Arrange
        using var sp = BuildIsolatedServiceProvider(nameof(DbSeeder_SeedsRolesAndAdminIdempotently));
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedAdmin:Email"] = "admin@test.local",
                ["SeedAdmin:Password"] = "TestPass123!"
            })
            .Build();

        // Act
        await DbSeeder.SeedRolesAndAdminAsync(sp, config);
        await DbSeeder.SeedRolesAndAdminAsync(sp, config); // second call must remain idempotent

        // Assert
        using var scope = sp.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            (await roleManager.RoleExistsAsync(role)).Should().BeTrue();
        }

        var admin = await userManager.FindByEmailAsync("admin@test.local");
        admin.Should().NotBeNull();
        (await userManager.IsInRoleAsync(admin!, Roles.Admin)).Should().BeTrue();
    }

    [Fact]
    public async Task DbSeeder_SeedsDemoDataIdempotently()
    {
        // Arrange
        using var sp = BuildIsolatedServiceProvider(nameof(DbSeeder_SeedsDemoDataIdempotently));
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedAdmin:Email"] = "admin@test.local",
                ["SeedAdmin:Password"] = "TestAdmin123!",
                ["SeedDemo:Password"] = "TestDemo123!"
            })
            .Build();

        // Act
        await DbSeeder.SeedDemoDataAsync(sp, config);
        await DbSeeder.SeedDemoDataAsync(sp, config); // second run should not duplicate

        // Assert
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        (await db.Hackathons.CountAsync()).Should().Be(1);
        (await db.Tracks.CountAsync()).Should().Be(3);
        (await db.Criteria.SumAsync(c => c.WeightPercent)).Should().Be(100);
        (await db.Teams.CountAsync()).Should().Be(3);
        (await db.Submissions.CountAsync()).Should().Be(3);
        (await db.Scores.CountAsync()).Should().Be(12);
    }
}
