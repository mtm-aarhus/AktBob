Består af to hosts: et web API og en worker service. Baggrundsprocessering af workflow processer håndteres ved brug af Hangfire.

Applikationen er opbygget som en "modular monolith". Hvert modul udstiller endpoints via et public interface, som applikationens andre moduler kan anvende via dependency injection. Web API'et og worker servicen er selvstændige applikationer, men anvender de samme moduler.

Der er ingen direkte kommunikationen mellem Web API'et og Worker'en, men der er en indirekte one-way kommunkation i form af at Web API'et opretter jobs i Hangfire, som worker applikationen afvikler. Web API'et ligger ude på internettet så de eksterne systemer (Deskpro og Podio) kan anvende det. Worker'en kører som en Windows service på en intern on-premise server. Der afvikles ingen Hangfire jobs i web API'et.

# AktBob API

## Hangfire Dashboard
Hangfire Dashboard kan tilgås via /hangfire. Dashboardet afvikles via API-host'en, og ligger dermed ude på internettet. Derfor kræves det browser credentials (brugernavn/password) at tilgå dashboardet. Credentials er konfigureret i appsettings (i produktion: environment variables).

## Azure Container Web App
API'et inkl. Hangfire Dashboard er deployet i produktion som en containerized Azure web app.



# AktBob Worker service
AktBob.Worker er en .NET console applikation, som kører i produktion som en Windows Service.

## Hangfire
Afviklingen af baggrundsjobs orkestreres af Hangfire. Hangfire anvender en database til at holde styr på det hele, og der skal angives en connectionstring i appsettings:

    "ConnectionString": {
        "Hangfire": "..."
    }

Der kan anvendes samme eller anden database, som databasemodulet anvender. Det skal bare være en MSSQL-database, da opsætningen er lavet med SqlClient. Ønskes en anden database, f.eks. SQLite, skal man lige ind i koden for at ændre hvilken client Hangfire anvender.

## Logging
AktBob.Worker logger en lang række beskeder på forskellige log levels (debug, info, warning, error). Logging sker via Serilog og det kan for de enkelte processer og moduler konfigureres i appsettings hvilke loglevels der ønskes (standard .NET logging feature).

Eksempel:

    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft.AspNetCore": "Warning",
                "System.NET.Http": "Warning",
                "Hangfire": "Information",
                "AktBob.Deskpro.DeskproModule":"Debug" 
            }
        }
    }

Logging er sat op til at kunne udsende mails indeholdende logevents fra og med warning-level og op. For at udsende mails konfigureres Serilog med disse appsettings:

    "EmailLogEvents": {
        "Enabled": true,
        "To": [
            "somebody@example.com"
        ],
        "From": "somebody@example.com",
        "Host": "localhost",
        "Port": 25,
        "TimeLimitMinutes": 1
    }

*TimeLimitMinutes* angiver den maksimale tid fra at der registreres en warning-, error- eller critical log event, til at der skal sendes en mail. 

## Moduler
Løsningen består af følgende moduler:

### CloudConvert Module
Konfiguration:

    "CloudConvert": {
        "BaseUrl": "",
        "Token": ""
    }

#### Endpoints
* **ConvertHTMLToPDF**: Sender HTTP request til CloudConvert, der igangsætter den egentlige konvertering. Endpointet skal kaldes med en dictionary af CloudConvert tasks og returnerer JobId'et fra CloudConvert (Guid). 
* **DownloadFile**: Downloader den færdigkonverterede fil fra CloudCovert. Skal kaldes med en URL string og returnerer filen som et byte array.
* **GetDownloadURL**: Requester CloudConvert efter URL'en til at downloade den færdigkonverterede fil. Endpointet requester CloudConvert hvert 2. sekund og returnerer URL'en, når den er klar. Endpointet skal kaldes med JobId'et. Hvis CloudConvertprocessen fejler, returneres et error result.
* **GenereateTasks**: Endpoint til at generere en dictionary med CloudConvert tasks. Skal kaldes med en IEnumerable af de filer, der skal konverteres i form af byte arrays.

### Database Module
Anvendes til interaktion med databasen. Udstiller en klassisk UnitOfWork bestående af 3 repositories:

* Tickets
* Cases
* Messages
    
Databasen holder styr på metadata fra de eksterne systemer og anvendes af workflowprocesserne. Databaseskemaet ligger i AktBob.Database.SQL-projektet.

Connectionstring til databasen skal angives i appsettings:

    "ConnectionStrings": {
        "Database": "..."
    }

### Deskpro Module
Deskpro modulet har en afhængighed til nugetpakken AAK.Deskpro, som håndterer de bagvedliggende HTTP-kald til Deskpro. Modulet konfigureres via appsettings:

    "Deskpro": {
        "BaseAddress": "string",
        "AuthorizationKey": "string",
        "GetPersonHandler": {
            "IgnoreEmails": [
                "somebody@example.com"
            ]
        },
        "Webhooks": {
            "UpdateTicketSetGoCaseId": "webhook key here",
            "SetGetOrganizedAggregatedCaseIds": "webhook key here"
        },
        "Fields": {
            "Afdeling": 0
        }
    }

#### Endpoints
* **InvokeWebhook**: Sender en request til et inbound webhook i Deskpro. Skal kaldes med ID'et på webhooket (ikke den fulde URL, kun ID'et) samt den payload, der skal sendes til Deskpro. Der er tale om et fire-and-forget endpoint, der ikke returnerer en værdi.
* **GetCustomFieldSpecifications**: Returnerer en ReadOnlyCollection med de custom ticket fields der findes i pågældende Deskpro instance. Anvender caching af 5 døgns varighed for at minimere unødvendige mange requests til Deskpro.
* **DownloadMessageAttachment**: Returner en specifikt Deskpro message attachment i form af en stream. Skal kaldes med en specifik download URL.
* **GetMessageAttachments**: Returnerer en ReadOnlyCollection med de attachments, der findes på en specifik message, inkl. download URL. Skal kaldes med ticketId og messageId. Anvender caching af 5 døgns varighed for at minimere unødvendige mange requests til Deskpro.
* **GetMessage**: Returnerer en specifik message. Skal kaldes med ticketId og messageId. Anvender caching af 5 døgns varighed for at minimere unødvendige mange requests til Deskpro.
* **GetMessages**: Returnerer en liste med ID'er på alle de messages, der findes på en ticket. Skal kaldes med ticketId.
* **GetPerson** (via ID): Returnerer et Deskpro person-objekt med en specifikke person. Returnerer en result error, hvis personen ikke findes. Skal kaldes med et personId. Anvender caching af 20 døgns varighed for at minimere  unødvendige mange requests til Deskpro.
* **GetPerson** (via email): Returnerer en collection med de Deskpro person-objekter, der kunne fremsøges via emailadresse. Anvender caching af 20 døgns varighed for at minimere unødvendige mange requests til Deskpro.
* **GetTicket**: Returnerer en Deskpro ticket.
* **GetTicketsByFieldSearch**: Returnerer et søgeresultat indeholdende de tickets, der har angivet value i de angivede felter. Skal kaldes med en searchValue og et array over de custom ticket fields, der skal søges i.

### Email Module
Email modulet anvendes ikke i øjeblikket, men er tiltænkt som en feature til at udsende emails i forbindelse med workflow processerne. Modulet konfigureres via appsettings:

    "EmailModule": {
        "From": "noreply@example.com",
        "SmtpUrl": "localhost",
        "SmtpPort": 25,
        "SmtpUseSsl": false
    }

#### Endpoints
* **Send**: Endpoint til at udsende en mail. Afsenderadresse konfigureres i appsettings. Skal kaldes med modtageradresse, emne og indhold. 

### GetOrganized Module
GetOrganized modulet har en afhængig til nugetpakken AAK.GetOrganized, som håndterer de bagliggende HTTP-kald til GetOrganized.

Konfiguration:

    "GetOrganized": {
        "BaseAddress": "",
        "Username": "",
        "Password": "",
        "Domain": ""
    }
    
#### Endpoints
* **FinalizeDocument**: Endpoint til at færdigjournalisere et dokument i GetOrganized.
* **CreateCase**: Opretter en ny sag i GetOrganized og returnerer en response indeholdende sagsnummeret og sagens URL.
* **RelateDocuments**: Forvandler et eller flere dokumenter til bilag relateret til et hoveddokument. Skal kaldes med dokumentnummeret på hoveddokumentet samt en liste af dokument ID'er, der skal sættes som bilag til hoveddokumentet.
* **UploadDocument**: Uploader et dokument til en GetOrganized sag og returnerer dokumentets documentId.
* **GetAggregatedCase**: Returnerer en liste over sager, der ligger på samlesag.

### OpenOrchestrator Module
Connectionstring til OpenOrchestrator skal angives i appsettings:
    
    "ConnectionStrings": {
        "OpenOrchestratorDb": "..."
    }

#### Endpoints
* **CreateQueueItem**: Tilføjet et nyt køelement i OpenOrchestrator. Returnerer ikke noget.

### Podio Module
Podio modulet har en afhængighed til nugetpakken AAK.Podio, som håndterer de bagvedliggende HTTP-kald til Podio.

Konfiguration:

    "Podio": {
        "AppId": 0,
        "BaseAddress": "",
        "ClientId": "",
        "ClientSecret": "",
        "AppTokens": {
            "0": "0"
        },
        "Fields": {
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

#### Endpoints
* **GetItem**: Henter et item fra Podio.
* **PostComment**: Tilføjer en ny kommentar på et Podio item.
* **UpdateTextField**: Opdaterer et felt af typen 'text' på et Podio item.

## Workflowprocesser
Alle workflowprocesser afvikles i AktBob.Worker som Hangfire baggrundsjob.
### Add Message To GetOrganized
### Add Or Update Deskpro Ticket To GetOrganized
### Check OCR Screening Status
Konfiguration:

    "CheckOCRScreeningStatus": {
        "UpdatePodioItemSetFilArkivUrlImmediately": true,
        "QueryFilesTimeoutHours": 48,
        "PollingIntervalMinutes": 30
    }

### Create "Document List" Queue Item
Konfiguration:

    "CreateDocumentListQueueItemJobHandler": {
        "OpenOrchestratorQueueName": ""
    }

### Create "Afgørelsesskrivelse" Queue Item
Konfiguration:

    "CreateAfgørelsesskrivelseQueueItemJobHandler": {
        "OpenOrchestratorQueueName": "",
        "AfdelingFieldId": 0
    }

### Create "Journalize Everything" Queue Item
Konfiguration:

    "CreateJournalizeEverythingQueueItemJobHandler": {
        "OpenOrchestratorQueueName": ""
    }

### Create "To FilArkiv" Queue Item
Konfiguration:
    
    "CreateToFilArkivQueueItemJobHandler": {
        "OpenOrchestratorQueueName": ""    
    }

### Create "To Sharepoint" Queue Item
Konfiguration:

    "CreateToSharepointQueueItemJobHandler": {
        "OpenOrchestratorQueueName": ""
    }

### Register Podio Cases
### Update Deskpro Set GetOrganized Aggregated Case Numbers


# Konfiguration

## AktBob.API

    {
        "ApiKey": "",
        "Podio": {
            "BaseAddress": "",
            "ClientId": "",
            "ClientSecret": "",
            "AppTokens": {
                "0": ""
            },
            "AktindsigtApp": {
                "Id": 0,
                "Fields": {
                    "Sharepointmappe": 0,
                    "Dokumentliste": 0
                }
            }
        },
        "ConnectionStrings": {
            "Database": ""
            "Hangfire": ""
        }
        "HangfireDashboard": {
            "Username": "",
            "Password": ""
        }
    }

## AktBob.Worker

    {
        "LogFilesPath": "",
        "EmailLogEvents": {
            "Enabled": true,
            "To": [
                ""
            ],
            "From": "",
            "Host": "",
            "Port": 25,
            "TimeLimitMinutes": 10
        },

        "Serilog": {
            "MinimumLevel": {
                "Default": "Information",
                "Override": {
                    "Microsoft.AspNetCore": "Warning",
                    "System.NET.Http": "Warning"
                }
            }
        },

        "ConnectionStrings": {
            "OpenOrchestratorDb": "",
            "Database": "",
            "Hangfire": ""
        },

        "Hangfire": {
            "Workers": 50
        },

        "FilArkiv": {
            "BaseAddress": "",
            "ClientId": "",
            "ClientSecret": "",
            "TokenEndpoint": "",
            "Scope": ""
        },


        "Deskpro": {
            "BaseAddress": "",
            "AuthorizationKey": "",
            "GetPersonHandler": {
                "IgnoreEmails": [
                    ""
                ]
            },
            "Webhooks": {
                "UpdateTicketSetGoCaseId": "",
                "SetGetOrganizedAggregatedCaseIds": ""
            },
            "Fields": {
                "Afdeling": 0
            }
        },

        "CloudConvert": {
            "BaseUrl": "",
            "Token": ""
        },

        "Podio": {
            "AppId": 0,
            "BaseAddress": "",
            "ClientId": "",
            "ClientSecret": "",
            "AppTokens": {
                "0": "0"
            },
            "Fields": {
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
        },
        
        "GetOrganized": {
            "BaseAddress": "",
            "Username": "",
            "Password": "",
            "Domain": ""
        },

        "CreateGetOrganizedCase": {
            "Facet": "",
            "CaseTypePrefix": "",
            "CaseStatus": "",
            "CaseAccess": "",
            "CaseProfile": "",
            "KleMapping": {
                "0": ""
            },
            "DepartmentMapping": {
                "0": "0"
            }
        },

        "CheckOCRScreeningStatus": {
            "UpdatePodioItemSetFilArkivUrlImmediately": true,
            "QueryFilesTimeoutHours": 48,
            "PollingIntervalMinutes": 30
        },
        
        "CreateDocumentListQueueItemJobHandler": {
            "OpenOrchestratorQueueName": ""
        },

        "CreateToFilArkivQueueItemJobHandler": {
            "OpenOrchestratorQueueName": ""    
        },

        "CreateToSharepointQueueItemJobHandler": {
            "OpenOrchestratorQueueName": ""
        },

        "CreateJournalizeEverythingQueueItemJobHandler": {
            "OpenOrchestratorQueueName": ""
        },

        "CreateAfgørelsesskrivelseQueueItemJobHandler": {
            "OpenOrchestratorQueueName": "",
            "AfdelingFieldId": 0
        },

        "EmailModule": {
            "From": "",
            "SmtpUrl": "",
            "SmtpPort": 25,
            "SmtpUseSsl": false
        }
    }