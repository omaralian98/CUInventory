# Docker Compose Setup - Generated from Aspire AppHost

This Docker Compose configuration is **generated from your .NET Aspire AppHost.cs** configuration.

## About This Setup

This `docker-compose.yml` file is automatically orchestrated from your Aspire application definition in `src/CUInventory.AppHost/AppHost.cs`. 

The services are configured to match your Aspire AppHost exactly:
- SQL Server (with persistent lifetime from `.WithLifetime(ContainerLifetime.Persistent)`)
- CloudBeaver database UI (from `.AddContainer("CloudBeaver", "dbeaver/cloudbeaver", "24.3.5")`)
- Database Migrator (from `.AddProject<Projects.CUInventory_DbMigrator>("Migrator")`)
- Backend API (from `.AddProject<Projects.CUInventory_HttpApi_Host>("Backend")`)
- Frontend Vite App (from `.AddViteApp("Frontend", "../../angular", ...)`)

## Quick Start

```bash
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Access Points

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **CloudBeaver DB UI**: http://localhost:8978
- **SQL Server**: localhost:1433

## Configuration

Edit `.env` to customize:
- `SqlPassword` - SQL Server SA password
- `CloudBeaverAdminPassword` - CloudBeaver admin password

## How This Was Generated

This configuration was created from your Aspire AppHost using these principles:

1. **Services**: Each resource in AppHost.cs is mapped to a service
2. **Dependencies**: Dependencies specified in AppHost.cs (`.WaitFor()`, `.WaitForIfNotNull()`) are reflected in `depends_on`
3. **Volumes**: Volumes from `.WithVolume()` and `.WithBindMount()` are preserved
4. **Environment**: Environment variables from `.WithEnvironment()` are set
5. **Networking**: All services use a shared Docker network for internal communication

## Aspire Features Preserved

✅ SQL Server with persistent volumes
✅ CloudBeaver configuration (volumes, environment variables)
✅ Service dependencies and startup order
✅ Health checks
✅ External HTTP endpoints
✅ Environment variable references

## For Production Deployment

When deploying to production:
1. Change all passwords in `.env` to strong, unique values
2. Configure SSL/TLS with a reverse proxy
3. Set `ASPNETCORE_ENVIRONMENT` to `Production` in `.env`
4. Implement proper backups for the `sqldata` volume
5. Set up monitoring and logging

## Notes

- This is a **Docker Compose deployment** generated from your Aspire configuration
- The Dockerfiles use **multi-stage builds** for optimized images
- Services communicate via the internal `cuinventory-network`
- Data persists in named volumes (`sqldata`, `cloudbeaver-workspace`)

