Applikationen består af to hosts: et web API og en worker service. Baggrundsprocessering af workflow processer håndteres ved brug af Hangfire.

Applikationen er opbygget som en "modular monolith". Hvert modul udstiller endpoints via et public interface,
som applikationens andre moduler anvender via dependency injection. 
Web API'et og worker servicen er selvstændige applikationer, men anvender de samme moduler.

Der er ingen direkte kommunikationen mellem Web API'et og Worker'en, men der er en indirekte one-way kommunkation 
i form af at Web API'et opretter jobs i Hangfire, som worker applikationen afvikler. 

Der er public adgang til web API'et, så de eksterne systemer (Deskpro og Podio) kan anvende det. 
Worker'en kører som en Windows service på en intern on-premise server. Der afvikles ingen jobs i web API'et.

# API
Hangfire Dashboard kan tilgås via `/hangfire`. Adgang til dashboardet kræver browser credentials (brugernavn/password).

Swagger dokumentation af API-endpoints via `/swagger`.


# Worker service

## Hangfire
Connectionstring til Hangfire database:

    "ConnectionString": {
        "Hangfire": "..."
    }

## Logging
AktBob.Worker logger beskeder på forskellige log levels (fatal, debug, info, warning, error). Logging sker via Serilog og det kan for de enkelte processer og moduler konfigureres i appsettings hvilke loglevels der ønskes.

    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft.AspNetCore": "Warning",
                "System.NET.Http": "Warning",
                "Hangfire": "Information",
                "AktBob.Deskpro":"Debug" 
            }
        }
    }

Der udsendes mails indeholdende logevents med error og critical events. Konfiguration:

    "EmailLogEvents": {
        "Enabled": true,
        "To": [
            "somebody@example.com",
            "somebody-else@example.com"
        ],
        "From": "somebody@example.com",
        "Host": "localhost",
        "Port": 25,
        "TimeLimitMinutes": 10
    }

*TimeLimitMinutes* angiver den maksimale tid fra at der registreres en error- eller critical log event, til at der skal sendes en mail. 

## Konfiguration af modulerne
### CloudConvert Module

    "CloudConvert": {
        "BaseUrl": "",
        "Token": ""
    }

### Database Module

    "ConnectionStrings": {
        "Database": "..."
    }

### Deskpro Module
Deskpro modulet har en afhængighed til AAK.Deskpro nugetpakken, som håndterer de bagvedliggende HTTP-kald til Deskpro.

    "Deskpro": {
        "BaseAddress": "",
        "AuthorizationKey": "",
        "GetPersonHandler": {
          "IgnoreEmails": [
            "somebody@example.com"
          ]
        },
        "Webhooks": {
          "UpdateTicketSetGoCaseId": "",
          "SetGetOrganizedAggregatedCaseIds": "",
          "UpdateDeskproSetFærdigbehandletDatoField": ""
        },
        "Fields": {
          "Afdeling": 0
        }
    }

### Email Module

    "EmailModule": {
        "From": "noreply@example.com",
        "SmtpUrl": "localhost",
        "SmtpPort": 25,
        "SmtpUseSsl": false
    }

### GetOrganized Module
GetOrganized modulet har en afhængig til AAK.GetOrganized nugetpakken, som håndterer de bagliggende HTTP-kald til GetOrganized.

    "GetOrganized": {
        "BaseAddress": "",
        "Username": "",
        "Password": "",
        "Domain": ""
    }

### OpenOrchestrator Module
<mark>TODO: dette skal omlægges til at anvende det nye API-endpoint i stedet</mark>

    "ConnectionStrings": {
        "OpenOrchestratorDb": "..."
    }

### Podio Module
Podio modulet har en afhængighed til AAK.Podio nugetpakken, som håndterer de bagvedliggende HTTP-kald til Podio.

Konfiguration:

    "Podio": {
        "AppId": 0,
        "BaseAddress": "",
        "ClientId": "",
        "ClientSecret": "",
        "AppTokens": {
          "0": "apptoken"
        },
        "Fields": {
          "0": {
            "AppId": 0,
            "Label": "SagsansvarligEmail"
          },
          "0": {
            "AppId": 0,
            "Label": "CaseNumber"
          },
          "0": {
            "AppId": 0,
            "Label": "FilArkivCaseId"
          },
          "0": {
            "AppId": 0,
            "Label": "FilArkivLink"
          },
          "0": {
            "AppId": 0,
            "Label": "DeskproId"
          }
        }
    }

## Workflowprocesser
<mark>TODO: beskriv processer og konfiguration</mark>