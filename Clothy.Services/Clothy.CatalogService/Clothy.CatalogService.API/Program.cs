using System.Reflection;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Mapper;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.DAL.UOW;
using DotNetEnv;
using FluentValidation.AspNetCore;
using FluentValidation;
using Clothy.CatalogService.BLL.FluentValidation.BrandValidation;
using Clothy.CatalogService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
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

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// REGISTER AUTO MAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TagProfile).Assembly);
});

// REGISTER SERVICES DI
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISizeService, SizeService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<IClothingTypeService, ClothingTypeService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IClotheService, ClotheService>();
builder.Services.AddScoped<IClothesStockService, ClothesStockService>();

// CLOUDINARY CONFIG
Env.Load();
builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME");
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY");
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET");
});

// Cloudinary DI registration
builder.Services.AddScoped<IImageService, ImageService>();

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(BrandCreateDTOValidator).Assembly);

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.AddNpgsqlDbContext<ClothyCatalogDbContext>("ClothyCatalogDb");

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();