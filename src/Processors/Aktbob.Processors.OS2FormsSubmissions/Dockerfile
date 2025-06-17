FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the NuGet configuration file and local NuGet packages
COPY nuget.config ./

COPY ["Processors/Aktbob.Processors.OS2FormsSubmissions/Aktbob.Processors.OS2FormsSubmissions.csproj", "Processors/Aktbob.Processors.OS2FormsSubmissions/"]
COPY ["AktBob.Shared/AktBob.Shared.csproj", "AktBob.Shared/"]
COPY ["AktBob.Database/AktBob.Database.csproj", "AktBob.Database/"]
RUN dotnet restore "./Processors/Aktbob.Processors.OS2FormsSubmissions/Aktbob.Processors.OS2FormsSubmissions.csproj"
COPY . .
WORKDIR "/src/Processors/Aktbob.Processors.OS2FormsSubmissions"
RUN dotnet build "./Aktbob.Processors.OS2FormsSubmissions.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aktbob.Processors.OS2FormsSubmissions.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
