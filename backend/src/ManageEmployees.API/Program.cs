using ManageEmployees.API.Extensions;
using ManageEmployees.API.Middlewares;
using ManageEmployees.Application;
using ManageEmployees.Infrastructure;
using ManageEmployees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// Configurações personalizadas
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Adicionar camadas da aplicação
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Aplicar migrations automaticamente e seed inicial
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Aplicando migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas com sucesso!");
        
        // Executar seed inicial (cria admin se não existir)
        logger.LogInformation("Executando seed inicial...");
        await seeder.SeedAsync();
        logger.LogInformation("Seed inicial concluído!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao aplicar migrations ou seed");
        throw;
    }
}

// Configure the HTTP request pipeline.
app.UseSwaggerConfiguration();

app.UseCors("AllowAll");

// Servir arquivos estáticos (fotos de perfil)
app.UseStaticFiles();

// Middleware de logging de requisições
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseGlobalExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("API iniciada na porta {Port}", builder.Configuration["Urls"] ?? "http://localhost:5000");

app.Run();

public partial class Program;
