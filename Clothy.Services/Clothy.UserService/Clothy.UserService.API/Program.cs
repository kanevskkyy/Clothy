using Clothy.UserService.BLL.Services.Interfaces;
using Clothy.UserService.BLL.Services;
using Clothy.UserService.DAL.DB;
using Clothy.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Clothy.UserService.DAL.Repositories.Interfaces;
using Clothy.UserService.DAL.Repositories;
using Clothy.Shared.Helpers;
using Clothy.UserService.BLL.Mapper;
using FluentValidation;
using Clothy.UserService.BLL.Validation.UserValidation;
using FluentValidation.AspNetCore;
using Clothy.UserService.API.Middleware;
using MassTransit;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Events.UserEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
   .AddDbContextCheck<UserDbContext>(
       name: "users-db-check",
       tags: new[] { "ready", "db", "postgres" },
       failureStatus: HealthStatus.Unhealthy);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<UserDbContext>("ClothyUsers");

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<UserDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager<SignInManager<ApplicationUser>>();

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(UserUpdateDTOValidation).Assembly);

// REGISTER AUTO MAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(UserProfile).Assembly);
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddCloudinary(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.Message<UserDeletedEvent>(e => e.SetEntityName("user-deleted"));
        cfg.Message<UserUpdatedEvent>(e => e.SetEntityName("user-updated"));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    UserDbContext db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    await UserSeeder.SeedAsync(db, userManager, roleManager);
}

app.UseAuthentication();
app.UseAuthorization();

app.UseServiceDefaults();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
