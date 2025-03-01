# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy the NuGet configuration file and local NuGet packages
COPY nuget.config ./

COPY ["AktBob.Api/AktBob.Api.csproj", "AktBob.Api/"]
COPY ["AktBob.Database/AktBob.Database.csproj", "AktBob.Database/"]
COPY ["AktBob.Shared/AktBob.Shared.csproj", "AktBob.Shared/"]
COPY ["AktBob.Deskpro.Contracts/AktBob.Deskpro.Contracts.csproj", "AktBob.Deskpro.Contracts/"]
COPY ["AktBob.Podio/AktBob.Podio.csproj", "AktBob.Podio/"]
COPY ["AktBob.Podio.Contracts/AktBob.Podio.Contracts.csproj", "AktBob.Podio.Contracts/"]

RUN dotnet restore "./AktBob.Api/AktBob.Api.csproj"
COPY . .

WORKDIR "/src/AktBob.Api"
RUN dotnet build "./AktBob.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./AktBob.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Use build argument to copy the right configuration file
ARG ENVIRONMENT=Production
COPY --from=build /app/publish .

# Copy the appsettings file based on the build argument
COPY AktBob.Api/appsettings.${ENVIRONMENT}.json ./appsettings.json

ENTRYPOINT ["dotnet", "AktBob.Api.dll"]