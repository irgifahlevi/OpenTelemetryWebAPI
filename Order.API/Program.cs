using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.API.Data;
using Order.API.Extension;
using Order.API.Repository;
using Order.API.Services;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("Default");
if (connection == null) throw new InvalidOperationException("Connection string 'Order API' not found.");

// Add services to the container.
builder.Services.AddHttpClient(); 
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();

var serviceVersion = builder.Configuration.GetSection("AppSettings")["ServiceVersion"];
var serviceName = builder.Configuration.GetSection("AppSettings")["ServiceName"];
var secretKey = builder.Configuration.GetSection("Honeycomb")["SecretKey"];


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName!, serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSqlClientInstrumentation()
        .AddConsoleExporter()
        .AddHoneycomb(o =>
        {
            o.ServiceName = serviceName;
            o.ServiceVersion = serviceVersion;
            o.ApiKey = secretKey!;
            o.Dataset = "Test";
        })
    );

var app = builder.Build();

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
