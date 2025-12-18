using Loom.Services.Configuration.Core;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Services;
using Loom.Services.Configuration.Api.Services;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Server;
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

builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCodeFirstGrpc();

builder.Services.AddScoped<CreateWorkflowDefinitionCommandHandler>();
builder.Services.AddScoped<CreateDraftWorkflowVersionCommandHandler>();
builder.Services.AddScoped<PublishWorkflowVersionCommandHandler>();
builder.Services.AddScoped<UpdateNodeMetadataCommandHandler>();
builder.Services.AddScoped<DeleteWorkflowVersionCommandHandler>();
builder.Services.AddScoped<ArchiveWorkflowDefinitionCommandHandler>();
builder.Services.AddScoped<AddNodeCommandHandler>();
builder.Services.AddScoped<UpdateNodeConfigCommandHandler>();
builder.Services.AddScoped<RemoveNodeCommandHandler>();
builder.Services.AddScoped<AddConnectionCommandHandler>();
builder.Services.AddScoped<RemoveConnectionCommandHandler>();
builder.Services.AddScoped<AddWorkflowVariableCommandHandler>();
builder.Services.AddScoped<UpdateWorkflowVariableCommandHandler>();
builder.Services.AddScoped<RemoveWorkflowVariableCommandHandler>();
builder.Services.AddScoped<AddWorkflowLabelDefinitionCommandHandler>();
builder.Services.AddScoped<RemoveWorkflowLabelDefinitionCommandHandler>();
builder.Services.AddScoped<CreateTriggerCommandHandler>();
builder.Services.AddScoped<UpdateTriggerConfigCommandHandler>();
builder.Services.AddScoped<DeleteTriggerCommandHandler>();
builder.Services.AddScoped<BindTriggerToWorkflowVersionCommandHandler>();
builder.Services.AddScoped<UnbindTriggerFromWorkflowVersionCommandHandler>();
builder.Services.AddScoped<BindTriggerToNodeCommandHandler>();
builder.Services.AddScoped<UnbindTriggerFromNodeCommandHandler>();

builder.Services.AddScoped<GetWorkflowDefinitionsQueryHandler>();
builder.Services.AddScoped<GetWorkflowVersionsQueryHandler>();
builder.Services.AddScoped<GetWorkflowVersionDetailsQueryHandler>();
builder.Services.AddScoped<GetCompiledWorkflowVersionQueryHandler>();
builder.Services.AddScoped<ValidateWorkflowVersionQueryHandler>();
builder.Services.AddScoped<GetWorkflowVersionsForTriggerQueryHandler>();

builder.Services.AddScoped<IWorkflowValidator, WorkflowValidator>();
builder.Services.AddScoped<IWorkflowCompiler, WorkflowCompiler>();

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
app.MapGrpcService<ConfigurationGrpcService>();

app.Run();
