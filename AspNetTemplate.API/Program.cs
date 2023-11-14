using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Debug, "[{Timestamp:HH:mm:ss} {Level:u3}][{Caller}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log.log", fileSizeLimitBytes: 1024 * 1024 * 10, rollOnFileSizeLimit: true, rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterMediatR(MediatRConfigurationBuilder
        .Create(Assembly.Load("ApiTestLogger.CQRS"))
        .WithAllOpenGenericHandlerTypesRegistered()
        .Build());
    /*containerBuilder.RegisterType<MariaDbContext>()
        .AsImplementedInterfaces()
        .As<DbContext>()
        .WithParameter(TypedParameter.From($"Server={EnvironmentData.Host};Port={EnvironmentData.Port};Database={EnvironmentData.Database};Uid={EnvironmentData.User};Pwd={EnvironmentData.Password};"));*/
    /*containerBuilder.RegisterType<TestLogService>().AsImplementedInterfaces().AsSelf();*/
    containerBuilder.RegisterInstance(Log.Logger).As<ILogger>();
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/*var scope = app.Services.CreateScope();
scope.ServiceProvider.GetRequiredService<DbContext>().Database.Migrate();*/


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.UseRouting();

app.Run();