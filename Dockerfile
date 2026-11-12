FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies using project file first for layer caching
COPY src/Clutch.Web/Clutch.Web.csproj src/Clutch.Web/
RUN dotnet restore src/Clutch.Web/Clutch.Web.csproj

# Copy source and publish Release build
COPY src/ src/
RUN dotnet publish src/Clutch.Web/Clutch.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Optional migration bundle stage: docker build --target migrator -t clutch-migrator .
FROM build AS migrator
RUN dotnet tool install --global dotnet-ef --version 8.*
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --project src/Clutch.Web/Clutch.Web.csproj --configuration Release --no-build --self-contained -r linux-x64 -o /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for container HEALTHCHECK
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Clutch.Web.dll"]
