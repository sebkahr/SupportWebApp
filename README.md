# IBAS Support – Blazor + Azure Cosmos DB

## Formål
En .NET Blazor Web App hvor IBAS' kunder kan oprette supporthenvendelser,
og hvor medarbejdere kan se dem alle samlet. Data gemmes i Azure Cosmos DB (NoSQL).
Lavet i faget Cloud Computing (M4.04).

## Arkitektur
Browser → Blazor Server-app → `CosmosSupportService` → Azure Cosmos DB

- `Models/SupportMessage.cs` – datamodel med validering (DataAnnotations)
- `Services/CosmosSupportService.cs` – al kommunikation med Cosmos DB
- `Components/Pages/CreateSupport.razor` – formular til oprettelse
- `Components/Pages/SupportList.razor` – tabel med alle henvendelser

Note: Den nyeste Blazor-template bruger `Components/Pages` og `Components/Layout`
i stedet for `Pages` og `Shared`, og har ingen `Controllers`.

## Designvalg
- **NoSQL / dokumentbaseret:** En henvendelse er ét selvstændigt, indlejret
  JSON-dokument uden behov for relationer. Skemaet kan udvides uden migrering.
- **Partitionsnøgle `/category`:** Henvendelser grupperes efter kategori.
- **Størrelse (M4.02):** Ca. 3-4 henvendelser/dag ≈ 1.000-1.500/år. Med ca. 2 KB pr.
  dokument er 3 års data under ca. 10 MB, så free tier er rigeligt.
- **SLA:** Cosmos DB tilbyder 99,99 % tilgængelighed for en enkelt region.
  Det er rigeligt til et supportsystem.

## Opret Cosmos DB med Azure CLI
Kør alle kommandoer i samme bash-terminal (variablerne genbruges).
```bash
az login
export RESGRP="IBasSupportRG"
export DBACCOUNT="ibas-db-account-$RANDOM"
export DATABASE="IBasSupportDB"
export CONTAINER="ibassupport"

az group create --name $RESGRP --location westeurope

az cosmosdb create --name $DBACCOUNT --resource-group $RESGRP \
  --enable-free-tier true

az cosmosdb sql database create --account-name $DBACCOUNT \
  --resource-group $RESGRP --name $DATABASE

# Windows/Git Bash: "//category"  |  Mac/Linux/PowerShell: "/category"
az cosmosdb sql container create --account-name $DBACCOUNT \
  --resource-group $RESGRP --database-name $DATABASE \
  --name $CONTAINER --partition-key-path "/category"

# Hent connection string
az cosmosdb keys list --name $DBACCOUNT --resource-group $RESGRP \
  --type connection-strings --query "connectionStrings[0].connectionString" -o tsv
```
Kun ét free-tier-abonnement er tilladt pr. Azure-abonnement. Fjern
`--enable-free-tier true`, hvis kommandoen fejler pga. dette.

## Kør projektet lokalt
```bash
git clone https://github.com/DITBRUGERNAVN/SupportWebApp.git
cd SupportWebApp
dotnet user-secrets init
dotnet user-secrets set "CosmosDb:ConnectionString" "<connection string>"
dotnet user-secrets set "CosmosDb:DatabaseName" "IBasSupportDB"
dotnet user-secrets set "CosmosDb:ContainerName" "ibassupport"
dotnet run
```
Connection strings ligger bevidst ikke i repoet.

## Status
**Nået:**
- Opret- og listeside med validering
- Data gemmes og hentes fra Cosmos DB
- Connection string håndteret med user secrets

**Mangler:**
- Filtrering på kategori, søgning, redigering og sletning
- Login/roller (alle kan i dag se alle henvendelser, hvilket er en sikkerhedsrisiko)
- Deployment til Azure
- Tests

**Forslag til næste trin:**
1. Deploy til Azure App Service med connection string i App Settings
2. Managed Identity i stedet for connection string
3. Login for medarbejdere på listesiden
4. CI/CD med GitHub Actions
