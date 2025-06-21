FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Processors/Aktbob.Processors.Cleanup/Aktbob.Processors.Cleanup.csproj", "Processors/Aktbob.Processors.Cleanup/"]
COPY ["AktBob.Shared/AktBob.Shared.csproj", "AktBob.Shared/"]
COPY ["AktBob.Database/AktBob.Database.csproj", "AktBob.Database/"]
RUN dotnet restore "./Processors/Aktbob.Processors.Cleanup/Aktbob.Processors.Cleanup.csproj"
COPY . .
WORKDIR "/src/Processors/Aktbob.Processors.Cleanup"
RUN dotnet build "./Aktbob.Processors.Cleanup.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aktbob.Processors.Cleanup.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
