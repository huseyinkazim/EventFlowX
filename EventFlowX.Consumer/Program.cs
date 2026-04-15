using EventFlowX.Consumer.Data;
using EventFlowX.Consumer.HostedService;
using EventFlowX.Infra.Messaging;
using EventFlowX.Infra.Messaging.Interface;
using EventFlowX.Shared.Services;
using Microsoft.EntityFrameworkCore;


var builder = Host.CreateApplicationBuilder(args);

var env = builder.Environment.EnvironmentName;

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("InboxDb");


builder.Services.AddDbContext<InboxDbContext>(option =>
{
    option.UseSqlServer(connectionString);
});


builder.Services.AddHostedService<Worker>();

var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                 ?? $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..6]}";

builder.Services.AddSingleton<IInstanceIdProvider>(_ => new InstanceIdProvider(instanceId));
builder.Services.AddSingleton<IRabbitMqConnection>(_ =>
    new RabbitMqConnection(builder.Configuration.GetConnectionString("RabbitMq")!));

builder.Services.AddSingleton<IEventSubscriber, RabbitMqSubscriber>();

var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InboxDbContext>();
    await db.Database.MigrateAsync();
}
host.Run();
