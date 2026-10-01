using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Serilog;
using TaskManager.Api.ExceptionHandling;
using TaskManager.Application;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)))
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Create);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskManager API",
        Version = "v1",
        Description = "API REST para gestão de tarefas: cadastro, listagem com filtros, edição e exclusão."
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(TaskResponse).Assembly.GetName().Name}.xml"));

    options.SupportNonNullableReferenceTypes();
    options.NonNullableReferenceTypesAsRequired();
});

builder.Services.AddHealthChecks();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();