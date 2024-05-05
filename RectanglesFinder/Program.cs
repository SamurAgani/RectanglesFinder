using FluentValidation.AspNetCore;
using RectanglesFinder.Middlewares;
using RectanglesFinder.Repositories;
using RectanglesFinder.Services.Interfaces;
using RectanglesFinder.Validators;
using Serilog;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["DbConnectionString"];

builder.Services.AddTransient<IRectangleRepository, RectangleRepository>(_ => new RectangleRepository(connectionString));
builder.Services.AddTransient<IRectangleService, RectangleService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Logging.ClearProviders();
builder.Host.UseSerilog(logger);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IRectangleService>();
    await service.SeedRectangles(100);
}
app.UseMiddleware<ExceptionHandlerMiddleware>();
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
