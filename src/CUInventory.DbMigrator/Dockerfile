# Multi-stage build for CUInventory.DbMigrator
# Generated from Aspire AppHost configuration

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CUInventory.sln", "."]
COPY ["common.props", "."]
COPY ["src/CUInventory.ServiceDefaults/", "src/CUInventory.ServiceDefaults/"]
COPY ["src/CUInventory.Domain.Shared/", "src/CUInventory.Domain.Shared/"]
COPY ["src/CUInventory.Domain/", "src/CUInventory.Domain/"]
COPY ["src/CUInventory.Application.Contracts/", "src/CUInventory.Application.Contracts/"]
COPY ["src/CUInventory.EntityFrameworkCore/", "src/CUInventory.EntityFrameworkCore/"]
COPY ["src/CUInventory.DbMigrator/", "src/CUInventory.DbMigrator/"]

# Restore dependencies
RUN dotnet restore "CUInventory.sln"

# Build the application
RUN dotnet build "src/CUInventory.DbMigrator/CUInventory.DbMigrator.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/CUInventory.DbMigrator/CUInventory.DbMigrator.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CUInventory.DbMigrator.dll"]