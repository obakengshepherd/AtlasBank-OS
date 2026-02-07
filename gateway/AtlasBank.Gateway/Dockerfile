# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["AtlasBank.sln", "."]
COPY ["gateway/AtlasBank.Gateway/AtlasBank.Gateway.csproj", "gateway/AtlasBank.Gateway/"]
COPY ["services/accounts/AtlasBank.Accounts.API/AtlasBank.Accounts.API.csproj", "services/accounts/AtlasBank.Accounts.API/"]
COPY ["services/transactions/AtlasBank.Transactions.API/AtlasBank.Transactions.API.csproj", "services/transactions/AtlasBank.Transactions.API/"]
COPY ["services/products/AtlasBank.Products.API/AtlasBank.Products.API.csproj", "services/products/AtlasBank.Products.API/"]
COPY ["services/compliance/AtlasBank.Compliance.API/AtlasBank.Compliance.API.csproj", "services/compliance/AtlasBank.Compliance.API/"]
COPY ["services/tenancy/AtlasBank.Tenancy.API/AtlasBank.Tenancy.API.csproj", "services/tenancy/AtlasBank.Tenancy.API/"]
COPY ["shared/AtlasBank.Core/AtlasBank.Core.csproj", "shared/AtlasBank.Core/"]
COPY ["shared/AtlasBank.Infrastructure/AtlasBank.Infrastructure.csproj", "shared/AtlasBank.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AtlasBank.sln"

# Copy remaining source code
COPY . .

# Build
RUN dotnet build "AtlasBank.sln" -c Release --no-restore

# Publish
RUN dotnet publish "gateway/AtlasBank.Gateway/AtlasBank.Gateway.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AtlasBank.Gateway.dll"]
