

using EventFlowX.Host.Data;
using EventFlowX.Host.HostedService;
using EventFlowX.Host.HostedService.Workers;
using EventFlowX.Host.HostedService.Workers.Interfaces;
using EventFlowX.Host.Publisher.Interface;
using EventFlowX.Shared.Models;
using EventFlowX.Shared.Services;
using EventFlowX.Workers.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("OutboxDb");

builder.Services.AddDbContext<OutboxDbContext>(option =>
{
    option.UseSqlServer(connectionString);
});

//hosted service
builder.Services.AddHostedService<PublisherService>();
builder.Services.AddHostedService<HeartbeatService>();

//worker service
builder.Services.AddScoped<IPublisherWorker, PublisherWorker>();
builder.Services.AddScoped<IHeartbeatWorker, HeartbeatWorker>();

var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                 ?? $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..6]}";

builder.Services.AddSingleton<IInstanceIdProvider>(_ => new InstanceIdProvider(instanceId));
builder.Services.AddSingleton<IEventPublisher>(sp =>
    {
        return RabbitMqPublisher.CreateAsync().GetAwaiter().GetResult();
    });


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
    await db.Database.MigrateAsync();
}
app.UseHttpsRedirection();
app.MapPost("/orders", async (OutboxDbContext context, CancellationToken cancellationToken) =>
{
    context.Add(new OutboxEvent
    {
        EventType = "OrderCreated",
        Payload = "{\"Message\": \"Hello, World!\"}",
    });
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok("Event published successfully.");
});

app.MapGet("/events", async (OutboxDbContext context) =>
{
    var result = await context.OutboxEvents
        .Include(e => e.Pod)
        .OrderByDescending(e => e.CreatedAt)
        .ToListAsync();
    return Results.Ok(result);
});

app.Run();

