using Clothy.AuthService.API.Middleware;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.Services;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.Shared.Helpers;
using FluentValidation.AspNetCore;
using FluentValidation;
using Clothy.AuthService.BLL.FluentValidation.Auth;

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

builder.Services.AddScoped<IKeycloakAuthService, KeycloakAuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(LoginDTOValidator).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseServiceDefaults();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
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
