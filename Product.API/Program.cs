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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName!, serviceVersion!))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddConsoleExporter()
        .AddHoneycomb(o =>
        {
            o.ServiceName = serviceName;
            o.ServiceVersion = serviceVersion;
            o.ApiKey = secretKey;
            o.Dataset = "Test";
        })
    );

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
