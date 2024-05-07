using FluentMigrator.Runner;
using FluentValidation.AspNetCore;
using RectanglesFinder.Middlewares;
using RectanglesFinder.Migrations;
using RectanglesFinder.Repositories;
using RectanglesFinder.Services.Interfaces;
using RectanglesFinder.Validators;
using Serilog;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["DbConnectionString"];

builder.Services.AddTransient<IRectangleRepository, RectangleRepository>(_ => new RectangleRepository(connectionString));
builder.Services.AddTransient<IRectangleService, RectangleService>();

DatabaseInitializer.EnsureDatabaseCreated(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog(logger);


builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CreateRectanglesTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();



using (var migrationScope = app.Services.CreateScope())
{
    var runner = migrationScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp(); 
}
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IRectangleService>();
    await service.SeedRectangles();
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

