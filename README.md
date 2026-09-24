# IBAS Support – Blazor + Azure Cosmos DB

## Gruppe
- Sebastian Hansen

## Formål
En .NET Blazor Web App hvor IBAS' kunder kan oprette supporthenvendelser,
og hvor medarbejdere kan se alle henvendelser samlet. Data gemmes i Azure Cosmos DB (NoSQL).
Lavet i faget Cloud Computing (opgave M4.04).

## Arkitektur
Browser → Blazor Server-app → `CosmosSupportService` → Azure Cosmos DB

- `Models/SupportMessage.cs` – datamodel med validering (DataAnnotations)
- `Services/CosmosSupportService.cs` – al kommunikation med Cosmos DB
- `Components/Pages/CreateSupport.razor` – formular til oprettelse
- `Components/Pages/SupportList.razor` – tabel med alle henvendelser
- `Components/Pages/Home.razor` – forside med links til de to sider

Note: Den nyeste Blazor-template bruger `Components/Pages` og `Components/Layout`
i stedet for `Pages` og `Shared`, og har ingen `Controllers`.

## Designvalg
- **NoSQL / dokumentbaseret:** En henvendelse er ét selvstændigt, indlejret
  JSON-dokument uden behov for relationer. Skemaet kan udvides uden migrering.
- **Partitionsnøgle `/category`:** Henvendelser grupperes efter kategori.
- **Størrelse:** Ca. 3-4 henvendelser om dagen giver ca. 1.000-1.500 om året.
  Med ca. 2 KB pr. dokument fylder 3 års data under ca. 10 MB.
- **SLA:** Et supportsystem kræver ca. 99,9 % oppetid. Cosmos DB lover 99,99 %
  for en enkelt region.
- **Sikkerhed:** Connection string ligger i *user secrets*, ikke i repoet.

## Opret Cosmos DB med Azure CLI
Kør alle kommandoer i samme bash-terminal (fx Azure Cloud Shell), da variablerne genbruges.

```bash
az login
export RESGRP="IBasSupportRG"
export DBACCOUNT="ibas-db-account-$RANDOM"
export DATABASE="IBasSupportDB"
export CONTAINER="ibassupport"

# 1. Resource group
az group create --name $RESGRP --location westeurope

# 2. Cosmos DB-konto (NoSQL API er standard)
az cosmosdb create --name $DBACCOUNT --resource-group $RESGRP \
  --locations regionName=swedencentral failoverPriority=0 isZoneRedundant=False \
  --enable-free-tier true

# 3. Database
az cosmosdb sql database create --account-name $DBACCOUNT \
  --resource-group $RESGRP --name $DATABASE

# 4. Container (partition key skal matche koden)
# Windows/Git Bash: "//category"  |  Mac/Linux/Cloud Shell: "/category"
az cosmosdb sql container create --account-name $DBACCOUNT \
  --resource-group $RESGRP --database-name $DATABASE \
  --name $CONTAINER --partition-key-path "/category"

# 5. Hent connection string
az cosmosdb keys list --name $DBACCOUNT --resource-group $RESGRP \
  --type connection-strings --query "connectionStrings[0].connectionString" -o tsv
```

Bemærkninger:
- Regionen `swedencentral` er valgt, fordi abonnementet kun tillader bestemte regioner
  (Polen, Østrig, UAE, Tyskland og Sverige). Skift til en tilladt region, hvis din politik er anderledes.
- Kun én konto pr. abonnement kan bruge free tier. Fjern `--enable-free-tier true`, hvis
  kommandoen fejler pga. dette.

## Kør projektet lokalt
Kræver .NET SDK 10 og adgang til en Cosmos DB oprettet som ovenfor.

```bash
git clone https://github.com/sebkahr/SupportWebApp.git
cd SupportWebApp
dotnet user-secrets init
dotnet user-secrets set "CosmosDb:ConnectionString" "<din connection string>"
dotnet user-secrets set "CosmosDb:DatabaseName" "IBasSupportDB"
dotnet user-secrets set "CosmosDb:ContainerName" "ibassupport"
dotnet run
```
Åbn den URL, der vises i konsollen (fx `http://localhost:5067`).
Connection strings ligger bevidst ikke i repoet.

## Status
**Nået (alle krav i M4.04):**
- Blazor Web App oprettet med `dotnet new blazor` og lagt på GitHub
- Modelklasse med validering
- Service der opretter og henter henvendelser i Azure Cosmos DB 
- Side til oprettelse af henvendelser med fejlbeskeder i UI'et
- Side der viser alle henvendelser i en tabel
- Navigation mellem siderne, og Counter/Weather er fjernet
- Forside tilpasset løsningen
- Connection string håndteret med user secrets

**Mangler / begrænsninger:**
- Ingen redigering, sletning eller statusændring af henvendelser
- Ingen filtrering eller søgning i listen
- Ingen login: alle kan se alle henvendelser, selvom de indeholder kontaktdata
- Appen kører kun lokalt og er ikke deployet til Azure
- Ingen automatiske tests

**Forslag til næste trin:**
1. Login for medarbejdere, så kun de kan se listen
2. Filtrering på kategori (partitionsnøglen gør det hurtigt)
3. Mulighed for at markere en henvendelse som behandlet
4. Deploy til Azure App Service med connection string i App Settings
5. Managed Identity i stedet for connection string
6. CI/CD med GitHub Actions
