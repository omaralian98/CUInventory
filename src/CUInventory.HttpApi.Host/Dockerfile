# Multi-stage build for CUInventory.HttpApi.Host
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
COPY ["src/CUInventory.Application/", "src/CUInventory.Application/"]
COPY ["src/CUInventory.EntityFrameworkCore/", "src/CUInventory.EntityFrameworkCore/"]
COPY ["src/CUInventory.HttpApi/", "src/CUInventory.HttpApi/"]
COPY ["src/CUInventory.HttpApi.Host/", "src/CUInventory.HttpApi.Host/"]

# Restore dependencies
RUN dotnet restore "CUInventory.sln"

# Build the application
RUN dotnet build "src/CUInventory.HttpApi.Host/CUInventory.HttpApi.Host.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/CUInventory.HttpApi.Host/CUInventory.HttpApi.Host.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "CUInventory.HttpApi.Host.dll"]