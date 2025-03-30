using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagement.MessageConsumer.Consumers;

var hostBuilder = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddMassTransit(config =>
        {
            config.AddConsumer<TaskCreatedConsumer>();
            config.AddConsumer<TaskUpdatedConsumer>();
            config.AddConsumer<TaskDeletedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
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