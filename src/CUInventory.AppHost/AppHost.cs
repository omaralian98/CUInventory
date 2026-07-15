using CUInventory.AppHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var useSqlContainer = builder.Configuration.GetValue("UseSqlContainer", true);

IResourceBuilder<IResourceWithConnectionString> database;
IResourceBuilder<IResource>? dbWait = null;

if (useSqlContainer)
{
    var sqlPassword = builder.AddParameter("SqlPassword", secret: true);
    var cloudBeaverAdminPassword = builder.AddParameter("CloudBeaverAdminPassword", secret: true);

    var sql = builder
        .AddSqlServer("sql", sqlPassword)
        .WithLifetime(ContainerLifetime.Persistent);
    dbWait = sql;
    database = sql.AddDatabase("Default", "CUInventory");

    builder
        .AddContainer("CloudBeaver", "dbeaver/cloudbeaver", "24.3.5")
        .WithHttpEndpoint(targetPort: 8978, name: "http")
        .WithExternalHttpEndpoints()
        .WithEnvironment("CB_SERVER_NAME", "CUInventory CloudBeaver")
        .WithEnvironment("CB_ADMIN_NAME", "cbadmin")
        .WithEnvironment("CB_ADMIN_PASSWORD", cloudBeaverAdminPassword)
        .WithEnvironment("CLOUDBEAVER_APP_GRANT_CONNECTIONS_ACCESS_TO_ANONYMOUS_TEAM", "true")
        .WithVolume("cloudbeaver-workspace", "/opt/cloudbeaver/workspace")
        .WithBindMount("cloudbeaver/data-sources.json",
            "/opt/cloudbeaver/workspace/GlobalConfiguration/.dbeaver/data-sources.json")
        .WaitFor(sql);
}
else
{
    database = builder.AddConnectionString("Default");
}


IResourceBuilder<ProjectResource>? migrator = null;
if (builder.Environment.IsDevelopment())
{
    migrator = builder
        .AddProject<Projects.CUInventory_DbMigrator>("Migrator")
        .WithReference(database)
        .WaitForIfNotNull(dbWait)
        .WithReplicas(1);
}


var backend = builder
    .AddProject<Projects.CUInventory_HttpApi_Host>("Backend")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WaitForIfNotNull(dbWait)
    .WaitForIfNotNull(migrator);

var frontend = builder
    .AddViteApp("Frontend", "../../angular", runScriptName: "start")
    .WithReference(backend)
    .WithHttpEndpoint(port: 4200)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck()
    .WithOtlpExporter()
    .WaitFor(backend);

builder.Build().Run();