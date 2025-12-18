using Loom.Services.Layout.Core;
using Loom.Services.Layout.Core.Commands.Handlers;
using Loom.Services.Layout.Core.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<LayoutDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<UpsertNodeLayoutCommandHandler>();
builder.Services.AddScoped<UpsertNodeLayoutsBatchCommandHandler>();
builder.Services.AddScoped<CopyLayoutFromWorkflowVersionCommandHandler>();
builder.Services.AddScoped<DeleteNodeLayoutCommandHandler>();

builder.Services.AddScoped<GetLayoutForWorkflowVersionQueryHandler>();
builder.Services.AddScoped<GetNodeLayoutQueryHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseHealthChecks("/health");

app.MapControllers();

app.Run();

