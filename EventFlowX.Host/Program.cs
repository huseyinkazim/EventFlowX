using EventFlowX.Host.Data;
using EventFlowX.Host.HostedService;
using EventFlowX.Shared.Models;
using EventFlowX.Shared.Services;
using EventFlowX.Shared.Shared;
using EventFlowX.Workers.Services;
using EventFlowX.Workers.Workers;
using EventFlowX.Workers.Workers.Interface;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

Batteries.Init();

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("OutboxDb");

var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
var dbPath = sqliteBuilder.DataSource;

var directory = Path.GetDirectoryName(dbPath);
if (!Directory.Exists(directory))
{
    Directory.CreateDirectory(directory!);
}

builder.Services.AddDbContext<OutboxDbContext>(option =>
{
    option.UseSqlite(connectionString);
});

//hosted service
builder.Services.AddHostedService<PublisherService>();
builder.Services.AddHostedService<MigrationService>();

//worker service
builder.Services.AddScoped<IPublisherWorker, PublisherWorker>();

var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                 ?? $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..6]}";

builder.Services.AddSingleton<IInstanceIdProvider>(_ => new InstanceIdProvider(instanceId));



var app = builder.Build();

app.MapPost("/publish", async (OutboxDbContext context) =>
{
    context.Add(new OutboxEvent
    {
        Id = Guid.NewGuid(),
        EventType = "TestEvent",
        Payload = "{\"Message\": \"Hello, World!\"}",
        Status = EventStatus.Pending,
        CreatedAt = DateTime.UtcNow
    });
    await context.SaveChangesAsync();
    return Results.Ok("Event published successfully.");
});


app.MapGet("", async (OutboxDbContext context,CancellationToken cancellationToken) =>
{

    var result = await context.OutboxEvents.ToListAsync();
    return Results.Ok(result);
});
app.UseHttpsRedirection();

app.Run();

