using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagement.MessageConsumer.Consumers;
using System;

var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddEnvironmentVariables(prefix: "TASKMANAGEMENT_");
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Get RabbitMQ configuration from environment variables with defaults
        var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "localhost";
        var rabbitMqUser = configuration["RABBITMQ_USER"] ?? "guest";
        var rabbitMqPassword = configuration["RABBITMQ_PASSWORD"] ?? "guest";

        Console.WriteLine($"RabbitMQ Connection: Host={rabbitMqHost}, User={rabbitMqUser}");

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