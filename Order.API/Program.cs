using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.API.Data;
var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("Default");
if (connection == null) throw new InvalidOperationException("Connection string 'Order API' not found.");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
