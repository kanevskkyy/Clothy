using Clothy.NotificationService.BLL.Configuration;
using Clothy.NotificationService.BLL.Services;
using MassTransit;
using Clothy.NotificationService.BLL.Services.Interfaces;
using DotNetEnv;
using Clothy.NotificationService.BLL.Consumers;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<EmailProviderOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("SENDGRID__KEY");
    options.FromEmail = Environment.GetEnvironmentVariable("SENDGRID__FROM_EMAIL");
    options.FromName = Environment.GetEnvironmentVariable("SENDGRID__FROM_NAME");
});

builder.Services.AddControllers();

builder.Services.AddScoped<IEmailProvider, SendGridProvider>();
builder.Services.AddScoped<ITemplateRender, RazorTemplateRender>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEmailConsumer>();
    x.AddConsumer<OrderDeliveredEmailConsumer>();
    x.AddConsumer<ClotheStockUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.ReceiveEndpoint("notification-order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedEmailConsumer>(context);
            e.Bind("send-notification-order-created");
        });

        cfg.ReceiveEndpoint("notification-clothe-stock-avaliable-queue", e =>
        {
            e.ConfigureConsumer<ClotheStockUpdatedConsumer>(context);
            e.Bind("clothe-stock-available");
        });

        cfg.ReceiveEndpoint("notification-order-delivered-queue", e =>
        {
            e.ConfigureConsumer<OrderDeliveredEmailConsumer>(context);
            e.Bind("send-notification-order-delivered");
        });
    });
});

var app = builder.Build();
app.UseServiceDefaults();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
