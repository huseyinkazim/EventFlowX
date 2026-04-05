using EventFlowX.Consumer.Consumer;
using EventFlowX.Consumer.Consumer.Interface;
using EventFlowX.Consumer.Data;
using EventFlowX.Consumer.HostedService;
using EventFlowX.Shared.Services;
using Microsoft.EntityFrameworkCore;


var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("InboxDb");


builder.Services.AddDbContext<InboxDbContext>(option =>
{
    option.UseSqlServer(connectionString);
});


builder.Services.AddHostedService<Worker>();

var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                 ?? $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..6]}";

builder.Services.AddSingleton<IInstanceIdProvider>(_ => new InstanceIdProvider(instanceId));
builder.Services.AddSingleton<IEventSubscriber>(sp =>
    RabbitMqConsumer.CreateAsync().GetAwaiter().GetResult());

var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InboxDbContext>();
    await db.Database.MigrateAsync();
}
host.Run();
