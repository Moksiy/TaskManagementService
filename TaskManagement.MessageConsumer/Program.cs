using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagement.MessageConsumer.Consumers;
using System;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // Add environment variables with optional prefix to configuration
        config.AddEnvironmentVariables(prefix: "TASKMANAGEMENT_");
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        logging.SetMinimumLevel(LogLevel.Information);

        logging.AddFilter("TaskManagement.MessageConsumer", LogLevel.Debug);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Get RabbitMQ configuration from environment variables with defaults
        var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "localhost";
        var rabbitMqUser = configuration["RABBITMQ_USER"] ?? "guest";
        var rabbitMqPassword = configuration["RABBITMQ_PASSWORD"] ?? "guest";

        services.AddMassTransit(config =>
        {
            // Register consumers
            config.AddConsumer<TaskCreatedConsumer>();
            config.AddConsumer<TaskUpdatedConsumer>();
            config.AddConsumer<TaskDeletedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPassword);
                });

                // Configure the endpoints for each consumer
                cfg.ReceiveEndpoint("task-created-queue", e =>
                {
                    e.ConfigureConsumer<TaskCreatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("task-updated-queue", e =>
                {
                    e.ConfigureConsumer<TaskUpdatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("task-deleted-queue", e =>
                {
                    e.ConfigureConsumer<TaskDeletedConsumer>(context);
                });
            });
        });

        // Configure OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddSource("TaskManagement.MessageConsumer")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TaskManagement.MessageConsumer"))
                .AddConsoleExporter());
    });

var host = hostBuilder.Build();
await host.RunAsync();