﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Sample Endpoint";

        var config = new EndpointConfiguration("SampleEndpoint");
        var transport = config.UseTransport<SqlServerTransport>();
        transport.ConnectionString(@"Server=.\SQLEXPRESS;Database=NServiceBus;Integrated Security=SSPI;Max Pool Size=100");

        config.UsePersistence<InMemoryPersistence>();
        config.EnableInstallers();

        config.AuditProcessedMessagesTo("audit");
        config.Recoverability()
            .Delayed(delayed => delayed.NumberOfRetries(0))
            .Immediate(immediate => immediate.NumberOfRetries(0));

        var endpoint = await Endpoint.Start(config)
            .ConfigureAwait(false);

        while (Console.ReadKey(true).Key != ConsoleKey.Escape)
        {
            await endpoint.SendLocal(new SomeMessage { CustomerId = Guid.NewGuid() })
                .ConfigureAwait(false);
        }

        await endpoint.Stop().ConfigureAwait(false);
    }
}

class SomeMessage
{
    public Guid CustomerId { get; set; }
}

class SomeMessageHandler : IHandleMessages<SomeMessage>
{
#pragma warning disable PS0018 // A task-returning method should have a CancellationToken parameter unless it has a parameter implementing ICancellableContext
    public Task Handle(SomeMessage message, IMessageHandlerContext context)
#pragma warning restore PS0018 // A task-returning method should have a CancellationToken parameter unless it has a parameter implementing ICancellableContext
    {
        Console.WriteLine($"Received {message.CustomerId} - ReplyTo: {context.MessageHeaders[Headers.ReplyToAddress]}");
        //throw new Exception("BAM!");
        return Task.CompletedTask;
    }
}
