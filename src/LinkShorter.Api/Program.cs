using LinkShorter.Api.Extensions;
using LinkShorter.Api.Filters;
using LinkShorter.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

builder
    .Services
    .AddHealthChecks()
    .AddDbContextCheck<DatabaseContext>();
builder.Services.AddControllers(
    c => c.Filters.Add<ExceptionFilter>());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("health");

await using var scope = app.Services.CreateAsyncScope();
await scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.MigrateAsync();

app.Run();