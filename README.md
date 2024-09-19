# AktBob.InternalWorker
Baggrundsservice i form af en Windows service, der pt. k�rer p� `SRVWEBSW03` under navnet `AktBob.InternalWorker` p� serverens Windows Services.
Den er konfigureret til starte automatisk i tilf�lde af at serveren genstartes.

Logfiler fra servicens gemmes lokalt p�  `SRVWEBSW03` i mappen `C:\Logs\AktBob.InternalWorker`. Der opbevares kun logfiler fra de seneste 30 dage.

# Konfiguration

    {
      "FilArkiv": {
        "BaseAddress": "https://core.filarkiv.dk",
        "ClientId": string,
        "ClientSecret": string,
        "TokenEndpoint": string,
        "Scope": "fa_de_api:normal"
      },
      "CheckOCRScreeningStatus": {
        "QueueName": "check-ocr-screening-status",
        "QueueVisibilityTimeoutSeconds": 60,
        "QueueMaxMessages": 10,
        "QueuePollingIntervalSeconds": 10,
        "MaxRandomDelayTimeMilliseconds": 3000,
        "DelayBetweenChecksSMilliSeconds": 3000
      },
      "JournalizeDeskproMessages": {
        "WorkerIntervalSeconds": 10,
        "JournalizeAfterUpload": true
      },
      "DocumentListTrigger": {
        "AzureQueueName": "document-list-trigger",
        "UiPathQueueName": {
          "Produktion": "AktbobGenererDokumentliste",
          "Udvikling": ""
        },
        "WorkerIntervalSeconds": 10
      },
      "GoToFilArkivTrigger": {
        "AzureQueueName": "go-to-filarkiv-trigger",
        "UiPathQueueName": {
          "Produktion": "AktbobGOToFilarkiv",
          "Udvikling": ""
        },
        "WorkerIntervalSeconds": 10
      },
      "ToSharepointTrigger": {
        "AzureQueueName": "to-sharepoint-trigger",
        "UiPathQueueName": {
          "Produktion": "AktbobFilarkivToSharePoint",
          "Udvikling": ""
        },
        "WorkerIntervalSeconds": 10
      },
      "RegisterCase": {
        "AzureQueueName": "register-podio-case",
        "WorkerIntervalSeconds": 10
      },
      "ConnectionStrings": {
        "AzureStorage": string
      },
      "EmailModule": {
        "From": "noreply@aarhus.dk",
        "Smtp": string,
        "IntervalSeconds": 30,
        "QueueName": "send-emails",
        "QueueVisibilityTimeoutSeconds": 300
      },
      "Podio": {
        "AppId": 29527765,
        "BaseAddress": "https://api.podio.com",
        "ClientId": string,
        "ClientSecret": string,
        "AppTokens": {
          "29527765": string
        },
        "Fields": {
          "262643381": {
            "AppId": 29527765,
            "Label": "CaseNumber"
          },
          "264622229": {
            "AppId": 29527765,
            "Label": "FilArkivCaseId"
          },
          "263817471": {
            "AppId": 29527765,
            "Label": "FilArkivLink"
          },
          "262643377": {
            "AppId": 29527765,
            "Label": "DeskproId"
          }
        }
      },
      "Deskpro": {
        "BaseAddress": "https://mtmsager.aarhuskommune.dk/api/v2/",
        "AuthorizationKey": string
      },
      "UiPath": {
        "Url": "https://orchestrator.adm.aarhuskommune.dk",
        "TenancyName": "Produktion",
        "Udvikling": {
          "Username": string,
          "Password": string,
          "OrganizationUnitId": 10
        },
        "Test": {
          "Username": string,
          "Password": string,
          "OrganizationUnitId": 0
        },
        "Produktion": {
          "Username": string,
          "Password": string,
          "OrganizationUnitId": 10145
        }
      },
      "DatabaseApi": {
        "Url": "http://localhost:8080/Api/", <-- erstat med produktions-URL
        "ApiKey": string
      },
      "GetOrganized": {
        "BaseAddress": string,
        "Username": string,
        "Password": string,
        "Domain": string
      }
    }


# Moduler

## AktBob.CheckOCRScreeningStatus
Servicen har til form�l at tjekke hvorvidt OCR-screeningsprocessen p� dokumenter, der er uploadet til FilArkiv, er f�rdig. N�r OCR-screeningen er f�rdig, opdaterer servicen det p�g�ldende Podio Item med link til sagen p� FilArkiv, udsender en notifikationsmail til den sagsansvarlige p� sagen i Deskpro samt poster en agent note p� sagen i Deskpro med oplysninger om at dokumenterne er OCR-screenet.

Servicen tjekker en Azure Queue (`check-ocr-screening-status`). K�en tjekkes hvert 10. sekund.

K�elementet skal indeholde oplysninger om hhv. Podio Item og FilArkiv Case ID i json-format:

    {
	    "filArkivCaseId": guid,
	    "podioItemId: long
    }

## AktBob.DocumentGenerator
... dokumentation mangler ...

## AktBob.JournalizeDocuments
... dokumentation mangler ...

## AktBob.Queue
... dokumentation mangler ...
