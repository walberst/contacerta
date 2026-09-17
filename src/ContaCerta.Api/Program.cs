using System.Text.Json.Serialization;
using ContaCerta.Application;
using ContaCerta.Infrastructure;
using ContaCerta.Infrastructure.Persistence;
using ContaCerta.Infrastructure.Realtime;
using ContaCerta.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration["Observabilidade:SeqUrl"] ?? "http://localhost:5341"));

var otlpEndpoint = builder.Configuration["Observabilidade:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ContaCerta.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var frontendUrl = builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:3000";
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(frontendUrl)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Comando de seed separado: dotnet run -- --seed. Roda as migrations, popula dados de
// demonstracao e encerra sem subir o servidor HTTP. Nunca acontece automaticamente.
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataRunner>();
    await seeder.ExecutarAsync();
    return;
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors("Frontend");

app.UseMiddleware<ContaCerta.Api.Middleware.ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHub<OrcamentoHub>("/hubs/orcamento");
app.MapHealthChecks("/health");

app.Run();

// Necessario para o WebApplicationFactory<Program> dos testes de integracao enxergar
// o entry point (top level statements geram uma classe Program interna por padrao).
public partial class Program
{
}
