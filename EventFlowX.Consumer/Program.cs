using EventFlowX.Consumer;
using EventFlowX.Consumer.Data;
using EventFlowX.Consumer.HostedService;
using EventFlowX.Shared.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
Batteries.Init();

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("InboxDb");

var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
var dbPath = sqliteBuilder.DataSource;

var directory = Path.GetDirectoryName(dbPath);
if (!Directory.Exists(directory))
{
    Directory.CreateDirectory(directory!);
}

builder.Services.AddDbContext<InboxDbContext>(option =>
{
    option.UseSqlite(connectionString);
});


builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<MigrationService>();


var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                 ?? $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..6]}";

builder.Services.AddSingleton<IInstanceIdProvider>(_ => new InstanceIdProvider(instanceId));

builder.Services.AddDbContext<InboxDbContext>(option =>
{
    option.UseSqlite(builder.Configuration.GetConnectionString("InboxDb"));
});

var host = builder.Build();
host.Run();
