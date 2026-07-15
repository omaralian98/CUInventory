using CUInventory.AppHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

const string backendPublicUrl = "http://localhost:8080";
const string frontendPublicUrl = "http://localhost:4200";

var useSqlContainer = builder.Configuration.GetValue("UseSqlContainer", true);

IResourceBuilder<IResourceWithConnectionString> database;
IResourceBuilder<IResource>? dbWait = null;

if (useSqlContainer)
{
    var sqlPassword = builder.AddParameter("SqlPassword", secret: true);
    var cloudBeaverAdminPassword = builder.AddParameter("CloudBeaverAdminPassword", secret: true);

    var sql = builder
        .AddSqlServer("sql", sqlPassword)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsDockerComposeService((_, service) =>
        {
            service.Restart = "unless-stopped";
            service.Healthcheck = new()
            {
                Test =
                [
                    "CMD-SHELL",
                    "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q \"SELECT 1\" -b"
                ],
                Interval = "5s",
                Timeout = "10s",
                Retries = 20,
                StartPeriod = "30s"
            };
        });
    dbWait = sql;
    database = sql.AddDatabase("Default", "CUInventory");

    builder
        .AddDockerfile("CloudBeaver", "cloudbeaver")
        .WithHttpEndpoint(port: 8978, targetPort: 8978, name: "http")
        .WithExternalHttpEndpoints()
        .WithEnvironment("CB_SERVER_NAME", "CUInventory CloudBeaver")
        .WithEnvironment("CB_ADMIN_NAME", "cbadmin")
        .WithEnvironment("CB_ADMIN_PASSWORD", cloudBeaverAdminPassword)
        .WithEnvironment("CLOUDBEAVER_APP_GRANT_CONNECTIONS_ACCESS_TO_ANONYMOUS_TEAM", "true")
        .WithVolume("cloudbeaver-workspace", "/opt/cloudbeaver/workspace")
        .WaitFor(sql)
        .PublishAsDockerComposeService((_, service) =>
        {
            service.Restart = "unless-stopped";
            service.Ports = ["8978:8978"];
            service.DependsOn["sql"].Condition = "service_healthy";
        });
}
else
{
    database = builder.AddConnectionString("Default");
}


IResourceBuilder<ProjectResource> migrator = builder
    .AddProject<Projects.CUInventory_DbMigrator>("Migrator")
    .WithReference(database)
    .WaitForIfNotNull(dbWait)
    .WithReplicas(1);

if (builder.ExecutionContext.IsPublishMode)
{
    migrator
        .WithEnvironment("OpenIddict__Applications__CUInventory_App__RootUrl", frontendPublicUrl)
        .WithEnvironment("OpenIddict__Applications__CUInventory_Swagger__RootUrl", $"{backendPublicUrl}/");
}

migrator.PublishAsDockerComposeService((_, service) =>
{
    service.DependsOn["sql"].Condition = "service_healthy";
});


var backend = builder
    .AddProject<Projects.CUInventory_HttpApi_Host>("Backend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health-status")
    .WithReference(database)
    .WaitForIfNotNull(dbWait)
    .WaitForCompletionIfNotNull(migrator);

if (builder.ExecutionContext.IsPublishMode)
{
    backend
        .WithEnvironment("App__SelfUrl", backendPublicUrl)
        .WithEnvironment("App__AngularUrl", frontendPublicUrl)
        .WithEnvironment("App__CorsOrigins", frontendPublicUrl)
        .WithEnvironment("App__RedirectAllowedUrls", frontendPublicUrl)
        .WithEnvironment("AuthServer__Authority", backendPublicUrl)
        .WithEnvironment("AuthServer__RequireHttpsMetadata", "false");
}

backend.PublishAsDockerComposeService((_, service) =>
{
    service.Restart = "unless-stopped";
    service.Environment["HTTP_PORTS"] = "8080";
    service.Ports = ["8080:8080"];
    service.DependsOn["sql"].Condition = "service_healthy";
});

if (builder.ExecutionContext.IsPublishMode)
{
    builder
        .AddDockerfile("Frontend", "../../angular")
        .WithHttpEndpoint(port: 4200, targetPort: 80)
        .WithExternalHttpEndpoints()
        .WaitFor(backend)
        .PublishAsDockerComposeService((_, service) =>
        {
            service.Restart = "unless-stopped";
            service.Ports = ["4200:80"];
        });
}
else
{
    builder
        .AddViteApp("Frontend", "../../angular", runScriptName: "start")
        .WithReference(backend)
        .WithHttpEndpoint(port: 4200)
        .WithExternalHttpEndpoints()
        .WithHttpHealthCheck()
        .WithOtlpExporter()
        .WaitFor(backend);
}

builder.Build().Run();
