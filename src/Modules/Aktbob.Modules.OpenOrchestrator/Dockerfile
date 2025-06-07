FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Modules/Aktbob.Modules.OpenOrchestrator/Aktbob.Modules.OpenOrchestrator.csproj", "Modules/Aktbob.Modules.OpenOrchestrator/"]
COPY ["AktBob.Shared/AktBob.Shared.csproj", "AktBob.Shared/"]

RUN dotnet restore "./Modules/Aktbob.Modules.OpenOrchestrator/Aktbob.Modules.OpenOrchestrator.csproj"
COPY . .

WORKDIR "/src/Modules/Aktbob.Modules.OpenOrchestrator"
RUN dotnet build "./Aktbob.Modules.OpenOrchestrator.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./Aktbob.Modules.OpenOrchestrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Use build argument to copy the right configuration file
ARG ENVIRONMENT=production
COPY --from=build /app/publish .

# Copy the appsettings file based on the build argument
COPY Modules/Aktbob.Modules.OpenOrchestrator/appsettings.${ENVIRONMENT}.json ./appsettings.json

ENTRYPOINT ["dotnet", "Aktbob.Modules.OpenOrchestrator.dll"]
