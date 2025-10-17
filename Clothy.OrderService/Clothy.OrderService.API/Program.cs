using System.Reflection;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.BLL.Mapper;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.UOW;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory(connectionString));

// REGISTER REPOSITORY
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
//

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//

// AUTO MAPPER REGISTER
builder.Services.AddAutoMapper(typeof(CityProfile).Assembly);
//

// SERVICES REGISTER
builder.Services.AddScoped<ICityService, CityService>();
//

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
