using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Product.API.Data;
using Product.API.Extension;
using Product.API.Repository;
using Product.API.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connection = builder.Configuration.GetConnectionString("Default");

if (connection == null) throw new InvalidOperationException("Connection string 'Order API' not found.");

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// register connection database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

var serviceVersion = builder.Configuration.GetSection("AppSettings")["ServiceVersion"];
var serviceName = builder.Configuration.GetSection("AppSettings")["ServiceName"];
var secretKey = builder.Configuration.GetSection("Honeycomb")["SecretKey"];
var host = builder.Configuration.GetSection("RabbitMQ")["HostName"];
var username = builder.Configuration.GetSection("RabbitMQ")["Username"];
var password = builder.Configuration.GetSection("RabbitMQ")["Password"];


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName!, serviceVersion!))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource(serviceName!)
        .AddConsoleExporter()
        .AddOtlpExporter()
        .AddHoneycomb(o =>
        {
            o.ServiceName = serviceName;
            o.ServiceVersion = serviceVersion;
            o.ApiKey = secretKey;
            o.Dataset = "Test";
        })
    );

builder.Services.AddMassTransit(o =>
{
    o.UsingRabbitMq((context, config) =>
    {
        config.Host(host!, c =>
        {
            c.Username(username!);
            c.Password(password!);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// register middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


