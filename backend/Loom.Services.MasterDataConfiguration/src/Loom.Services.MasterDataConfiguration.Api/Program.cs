using Loom.Services.MasterDataConfiguration.Core;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Services;
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MasterDataConfigurationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Command Handlers
builder.Services.AddScoped<CreateDataModelCommandHandler>();
builder.Services.AddScoped<CreateDataSchemaCommandHandler>();
        builder.Services.AddScoped<AddFieldDefinitionCommandHandler>();
        builder.Services.AddScoped<RemoveFieldDefinitionCommandHandler>();
        builder.Services.AddScoped<UpdateFieldDefinitionCommandHandler>();
        builder.Services.AddScoped<PublishDataSchemaCommandHandler>();
        builder.Services.AddScoped<DeleteSchemaVersionCommandHandler>();
        builder.Services.AddScoped<DeleteSchemaCommandHandler>();
        builder.Services.AddScoped<AddSchemaTagCommandHandler>();
        builder.Services.AddScoped<RemoveSchemaTagCommandHandler>();
        builder.Services.AddScoped<RemoveSchemaTagByValueCommandHandler>();
builder.Services.AddScoped<CreateValidationSpecCommandHandler>();
        builder.Services.AddScoped<AddValidationRuleCommandHandler>();
        builder.Services.AddScoped<RemoveValidationRuleCommandHandler>();
        builder.Services.AddScoped<UpdateValidationRuleCommandHandler>();
        builder.Services.AddScoped<AddValidationReferenceCommandHandler>();
builder.Services.AddScoped<PublishValidationSpecCommandHandler>();
builder.Services.AddScoped<CreateTransformationSpecCommandHandler>();
        builder.Services.AddScoped<AddSimpleTransformRuleCommandHandler>();
        builder.Services.AddScoped<RemoveSimpleTransformRuleCommandHandler>();
        builder.Services.AddScoped<UpdateSimpleTransformRuleCommandHandler>();
builder.Services.AddScoped<AddTransformReferenceCommandHandler>();
builder.Services.AddScoped<AddTransformGraphNodeCommandHandler>();
builder.Services.AddScoped<AddTransformGraphEdgeCommandHandler>();
builder.Services.AddScoped<AddTransformOutputBindingCommandHandler>();
builder.Services.AddScoped<RemoveTransformGraphNodeCommandHandler>();
builder.Services.AddScoped<RemoveTransformGraphEdgeCommandHandler>();
builder.Services.AddScoped<PublishTransformationSpecCommandHandler>();
        builder.Services.AddScoped<AddKeyDefinitionCommandHandler>();
        builder.Services.AddScoped<RemoveKeyDefinitionCommandHandler>();
        builder.Services.AddScoped<AddKeyFieldCommandHandler>();
        builder.Services.AddScoped<RemoveKeyFieldCommandHandler>();
        builder.Services.AddScoped<ReorderKeyFieldsCommandHandler>();

// Query Handlers
builder.Services.AddScoped<GetSchemasQueryHandler>();
        builder.Services.AddScoped<GetSchemaDetailsQueryHandler>();
        builder.Services.AddScoped<GetSchemaGraphQueryHandler>();
        builder.Services.AddScoped<ValidateSchemaQueryHandler>();
        builder.Services.AddScoped<GetUnpublishedDependenciesQueryHandler>();
        builder.Services.AddScoped<PublishRelatedSchemasCommandHandler>();
        builder.Services.AddScoped<GetValidationSpecDetailsQueryHandler>();
        builder.Services.AddScoped<GetValidationSpecBySchemaIdQueryHandler>();
        builder.Services.AddScoped<ValidateValidationSpecQueryHandler>();
        builder.Services.AddScoped<GetTransformationSpecDetailsQueryHandler>();
        builder.Services.AddScoped<GetTransformationSpecBySourceSchemaIdQueryHandler>();
        builder.Services.AddScoped<ValidateTransformationSpecQueryHandler>();
        builder.Services.AddScoped<GetCompiledTransformationSpecQueryHandler>();

// Services
builder.Services.AddScoped<IStaticValidationEngine, StaticValidationEngine>();

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

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
