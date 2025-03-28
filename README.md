Består af to hosts: et web API og en worker service. Baggrundsprocessering af workflow processer håndteres ved brug af Hangfire.

Applikationen er opbygget som en "modular monolith". Hvert modul udstiller endpoints via et public interface, som applikationens andre moduler kan anvende via dependency injection. Web API'et og worker servicen er selvstændige applikationer, men anvender de samme moduler.

Der er ingen direkte kommunikationen mellem Web API'et og Worker'en, men der er en indirekte one-way kommunkation i form af at Web API'et opretter jobs i Hangfire, som worker applikationen afvikler. Web API'et ligger ude på internettet så de eksterne systemer (Deskpro og Podio) kan anvende det. Worker'en kører som en Windows service på en intern on-premise server. Der afvikles ingen Hangfire jobs ude i Web API'et.

# AktBob API

## Hangfire Dashboard
Hangfire Dashboard kan tilgås via /hangfire. Dashboardet afvikles via API-host'en, og ligger dermed ude på internettet. Derfor kræves det browser credentials (brugernavn/password) at tilgå dashboardet. Credentials er konfigureret i appsettings (i produktion: environment variables).

## Azure Container Web App
API'et inkl. Hangfire Dashboard er deployet i produktion som en containerized Azure web app.



# AktBob Worker service
AktBob.Worker er en .NET console applikation, som kører i produktion som en Windows Service.


## Moduler
Løsningen består af følgende moduler:

### CloudConvert Module
* **ConvertHTMLToPDF**: Sender HTTP request til CloudConvert, der igangsætter den egentlige konvertering. Endpointet skal kaldes med en dictionary af CloudConvert tasks og returnerer JobId'et fra CloudConvert (Guid). 
* **DownloadFile**: Downloader den færdigkonverterede fil fra CloudCovert. Skal kaldes med en URL string og returnerer filen som et byte array.
* **GetDownloadURL**: Requester CloudConvert efter URL'en til at downloade den færdigkonverterede fil. Endpointet requester CloudConvert hvert 2. sekund og returnerer URL'en, når den er klar. Endpointet skal kaldes med JobId'et. Hvis CloudConvertprocessen fejler, returneres et error result.
* **GenereateTasks**: Endpoint til at generere en dictionary med CloudConvert tasks. Skal kaldes med en IEnumerable af de filer, der skal konverteres i form af byte arrays.

* **Database Module**: Anvendes til interaktion med databasen. Udstiller en klassisk UnitOfWork bestående af 3 repositories:
    * Tickets
    * Cases
    * Messages
    
    Databasen holder styr på metadata fra de eksterne systemer og anvendes af workflowprocesserne. Databaseskemaet ligger i AktBob.Database.SQL-projektet.

### Deskpro Module
Deskpro modulet har en afhængighed til nugetpakken AAK.Deskpro, som håndterer de bagvedliggende HTTP-kald til Deskpro.

* **InvokeWebhook**:
* **GetCustomFieldSpecifications**:
* **DownloadMessageAttachment**:
* **GetMessageAttachments**:
* **GetMessage**:
* **GetMessages**:
* **GetPerson** (via ID):
* **GetPerson** (via email):
* **GetTicket**:
* **GetTicketsByFieldSearch**:

### Email Module
* **Send**:

### GetOrganized Module
* **FinalizeDocument**:
* **CreateCase**:
* **RelateDocuments**:
* **UploadDocument**:
* **GetAggregatedCase**:

### OpenOrchestrator Module
* **CreateQueueItem**:

### Podio Module

Podio modulet har en afhængighed til nugetpakken AAK.Podio, som håndterer de bagvedliggende HTTP-kald til Podio.

* **GetItem**:
* **PostComment**:
* **UpdateTextField**:

## Workflowprocesser
Alle workflowprocesser afvikles i AktBob.Worker som Hangfire baggrundsjob.
### Add Message To GetOrganized
### Add Or Update Deskpro Ticket To GetOrganized
### Check OCR Screening Status
### Create "Document List" Queue Item
### Create "Afgørelsesskrivelse" Queue Item
### Create "Journalize Everything" Queue Item
### Create "To FilArkiv" Queue Item
### Create "To Sharepoint" Queue Item
### Register Podio Cases
### Update Deskpro Set GetOrganized Aggregated Case Numbers
