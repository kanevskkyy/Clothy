using System.Reflection;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Mapper;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.DAL.UOW;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// REGISTER REPOSITORIES DI
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IClotheItemRepository, ClotheItemRepository>();
builder.Services.AddScoped<IClothesStockRepository, ClothesStockRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IClothingTypeRepository, ClothingTypeRepository>();
//

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//

// REGISTER AUTO MAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TagProfile).Assembly);
});

//

// REGISTER SERVICES DI
builder.Services.AddScoped<ITagService, TagService>();
//

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<ClothyCatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ClothyCatalogDb")));

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
