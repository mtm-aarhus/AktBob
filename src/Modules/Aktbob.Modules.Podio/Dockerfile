FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy the NuGet configuration file and local NuGet packages
COPY nuget.config ./

COPY ["Modules/Aktbob.Modules.Podio/Aktbob.Modules.Podio.csproj", "Modules/Aktbob.Modules.Podio/"]
COPY ["AktBob.Shared/AktBob.Shared.csproj", "AktBob.Shared/"]

RUN dotnet restore "./Modules/Aktbob.Modules.Podio/Aktbob.Modules.Podio.csproj"
COPY . .

WORKDIR "/src/Modules/Aktbob.Modules.Podio"
RUN dotnet build "./Aktbob.Modules.Podio.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./Aktbob.Modules.Podio.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Use build argument to copy the right configuration file
ARG ENVIRONMENT=production
COPY --from=build /app/publish .

# Copy the appsettings file based on the build argument
COPY Modules/Aktbob.Modules.Podio/appsettings.${ENVIRONMENT}.json ./appsettings.json

ENTRYPOINT ["dotnet", "Aktbob.Modules.Podio.dll"]
