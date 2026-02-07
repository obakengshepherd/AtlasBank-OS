FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AtlasBank.sln", "."]
COPY ["shared/", "shared/"]
COPY ["services/tenancy/", "services/tenancy/"]

RUN dotnet restore "AtlasBank.sln"
RUN dotnet build "AtlasBank.sln" -c Release --no-restore
RUN dotnet publish "services/tenancy/AtlasBank.Tenancy.API/AtlasBank.Tenancy.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AtlasBank.Tenancy.API.dll"]
