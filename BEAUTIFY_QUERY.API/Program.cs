using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.API.DependencyInjection.Extensions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PERSISTENCE.DependencyInjection.Options;
using BEAUTIFY_QUERY.API.Middlewares;
using BEAUTIFY_QUERY.APPLICATION.DependencyInjection.Extensions;
using BEAUTIFY_QUERY.INFRASTRUCTURE.DependencyInjection.Extensions;
using BEAUTIFY_QUERY.PERSISTENCE.DependencyInjection.Extensions;
using Carter;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog();

// Add Carter module
builder.Services.AddCarter();

builder.Services
    .AddSwaggerGenNewtonsoftSupport()
    .AddFluentValidationRulesToSwagger()
    .AddEndpointsApiExplorer()
    .AddSwaggerAPI();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.ConfigureCors();

// Application Layer
builder.Services.AddMediatRApplication();
builder.Services.AddAutoMapperApplication();

// Persistence Layer
builder.Services.ConfigureSqlServerRetryOptionsPersistence(
    builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));
builder.Services.AddSqlServerPersistence();
builder.Services.AddRepositoryPersistence();
builder.Services.ConfigureServicesInfrastructure(builder.Configuration);

// Infrastructure Layer
builder.Services.AddServicesInfrastructure();
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddMediatRInfrastructure();
builder.Services.AddMasstransitRabbitMqInfrastructure(builder.Configuration);
// builder.Services.ConfigureHealthChecks(builder.Configuration);
builder.Services.AddJwtAuthenticationAPI(builder.Configuration);

builder.Services.AddTransient<ExceptionHandlingMiddleware>();
// builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Using middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline. 
// if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
app.UseSwaggerAPI(); // => After MapCarter => Show Version

app.UseCors("CorsPolicy");

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Need to be before app.UseAuthorization();
app.UseAuthorization();
// app.MapDefaultHealthChecks();
// app.MapDefaultHealthChecksUI();

// 7. Map Carter endpoints
app.MapCarter();

try
{
    await app.RunAsync();
    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
    await app.StopAsync();
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Program
{
}