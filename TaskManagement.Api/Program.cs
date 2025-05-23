﻿using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagement.Api.Endpoints;
using TaskManagement.Api.Middleware;
using TaskManagement.Application.Services;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Application.Services.Interfaces;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;
using TaskManagement.Infrastructure.Data.UnitOfWork;
using AutoMapper;
using TaskManagement.Application.DTOs.Mapping;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure the application to prioritize environment variables explicitly
builder.Configuration.AddEnvironmentVariables(prefix: "TASKMANAGEMENT_");

var services = builder.Services;

// Get configuration values from environment variables with defaults
var postgresHost = builder.Configuration["POSTGRES_HOST"] ?? "localhost";
var postgresPort = builder.Configuration["POSTGRES_PORT"] ?? "5432";
var postgresDb = builder.Configuration["POSTGRES_DB"] ?? "taskmanagement";
var postgresUser = builder.Configuration["POSTGRES_USER"] ?? "postgres";
var postgresPassword = builder.Configuration["POSTGRES_PASSWORD"] ?? "postgres";

var rabbitMqHost = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";
var rabbitMqUser = builder.Configuration["RABBITMQ_USER"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest";

// Construct connection string from environment variables
var connectionString = $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";

// Configure services
services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories and unit of work
services.AddScoped<ITaskRepository, TaskRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register application services
services.AddScoped<ITaskService, TaskService>();

// Register validators
services.AddScoped<IValidator<CreateTaskDto>, CreateTaskDtoValidator>();
services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskDtoValidator>();

services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "A simple API for managing user tasks"
    });

    var xmlFile = "TaskManagement.API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("TaskManagement.API")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TaskManagement.API"))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    });

services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new TaskMappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
services.AddSingleton(mapper);

var app = builder.Build();

// Add error handling middleware
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
app.UseCors();

var taskEndpoints = new TaskEndpoints();
taskEndpoints.Configure(app);

app.Run();