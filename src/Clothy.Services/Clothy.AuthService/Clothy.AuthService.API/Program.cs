using Clothy.AuthService.API.Middleware;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.Services;
using Clothy.AuthService.BLL.Services.Interfaces;
using MassTransit;
using FluentValidation.AspNetCore;
using FluentValidation;
using Clothy.AuthService.BLL.FluentValidation.Auth;
using Clothy.Shared.Events.UserEvents;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.AuthService.BLL.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCloudinary(builder.Configuration);

builder.Services.Configure<KeycloakSettings>(options =>
{
    options.Url = Environment.GetEnvironmentVariable("KEYCLOAK__URL") ?? builder.Configuration["Keycloak:Url"];
    options.Realm = Environment.GetEnvironmentVariable("KEYCLOAK__REALM") ?? builder.Configuration["Keycloak:Realm"];
    options.ClientId = Environment.GetEnvironmentVariable("KEYCLOAK__CLIENTID") ?? builder.Configuration["Keycloak:ClientId"];
    options.ClientSecret = Environment.GetEnvironmentVariable("KEYCLOAK__CLIENTSECRET") ?? builder.Configuration["Keycloak:ClientSecret"];
});

builder.Services.AddControllers();

builder.Services.AddScoped<IKeycloakAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKeycloakUserHelper, KeycloakUserHelper>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(LoginDTOValidator).Assembly);

builder.AddRedisClient("clothy-redis");

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(KeycloakMappingProfile).Assembly);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.Message<UserUpdatedEvent>(e => e.SetEntityName("user-updated-event"));


        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.UseServiceDefaults();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
