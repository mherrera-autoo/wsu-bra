# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution file
COPY ERP.sln .

# Copy all csproj files for dependency resolution
COPY ERP.Api/ERP.Api.csproj ERP.Api/
COPY ERP.Modules.Accounting/ERP.Modules.Accounting.csproj ERP.Modules.Accounting/
COPY ERP.Modules.Accounting.Contracts/ERP.Modules.Accounting.Contracts.csproj ERP.Modules.Accounting.Contracts/
COPY ERP.Modules.Billing/ERP.Modules.Billing.csproj ERP.Modules.Billing/
COPY ERP.Modules.Billing.Contracts/ERP.Modules.Billing.Contracts.csproj ERP.Modules.Billing.Contracts/
COPY ERP.Modules.Cash/ERP.Modules.Cash.csproj ERP.Modules.Cash/
COPY ERP.Modules.Cash.Contracts/ERP.Modules.Cash.Contracts.csproj ERP.Modules.Cash.Contracts/
COPY ERP.Modules.Finance/ERP.Modules.Finance.csproj ERP.Modules.Finance/
COPY ERP.Modules.Finance.Contracts/ERP.Modules.Finance.Contracts.csproj ERP.Modules.Finance.Contracts/
COPY ERP.Modules.FixedAssets/ERP.Modules.FixedAssets.csproj ERP.Modules.FixedAssets/
COPY ERP.Modules.FixedAssets.Contracts/ERP.Modules.FixedAssets.Contracts.csproj ERP.Modules.FixedAssets.Contracts/
COPY ERP.Modules.Identity/ERP.Modules.Identity.csproj ERP.Modules.Identity/
COPY ERP.Modules.Identity.Contracts/ERP.Modules.Identity.Contracts.csproj ERP.Modules.Identity.Contracts/
COPY ERP.Modules.Integrations/ERP.Modules.Integrations.csproj ERP.Modules.Integrations/
COPY ERP.Modules.Integrations.Contracts/ERP.Modules.Integrations.Contracts.csproj ERP.Modules.Integrations.Contracts/
COPY ERP.Modules.Subscriptions/ERP.Modules.Subscriptions.csproj ERP.Modules.Subscriptions/
COPY ERP.Modules.Inventory/ERP.Modules.Inventory.csproj ERP.Modules.Inventory/
COPY ERP.Modules.Inventory.Contracts/ERP.Modules.Inventory.Contracts.csproj ERP.Modules.Inventory.Contracts/
COPY ERP.Modules.MasterData/ERP.Modules.MasterData.csproj ERP.Modules.MasterData/
COPY ERP.Modules.MasterData.Contracts/ERP.Modules.MasterData.Contracts.csproj ERP.Modules.MasterData.Contracts/
COPY ERP.Modules.PharmaceuticalRegulatedInventory/ERP.Modules.PharmaceuticalRegulatedInventory.csproj ERP.Modules.PharmaceuticalRegulatedInventory/
COPY ERP.Modules.PharmaceuticalRegulatedInventory.Contracts/ERP.Modules.PharmaceuticalRegulatedInventory.Contracts.csproj ERP.Modules.PharmaceuticalRegulatedInventory.Contracts/
COPY ERP.Modules.Pricing/ERP.Modules.Pricing.csproj ERP.Modules.Pricing/
COPY ERP.Modules.Pricing.Contracts/ERP.Modules.Pricing.Contracts.csproj ERP.Modules.Pricing.Contracts/
COPY ERP.Modules.Purchasing/ERP.Modules.Purchasing.csproj ERP.Modules.Purchasing/
COPY ERP.Modules.Purchasing.Contracts/ERP.Modules.Purchasing.Contracts.csproj ERP.Modules.Purchasing.Contracts/
COPY ERP.Modules.Sales/ERP.Modules.Sales.csproj ERP.Modules.Sales/
COPY ERP.Modules.Sales.Contracts/ERP.Modules.Sales.Contracts.csproj ERP.Modules.Sales.Contracts/
COPY ERP.Modules.Tax/ERP.Modules.Tax.csproj ERP.Modules.Tax/
COPY ERP.Modules.Tax.Contracts/ERP.Modules.Tax.Contracts.csproj ERP.Modules.Tax.Contracts/
COPY ERP.Modules.Users/ERP.Modules.Users.csproj ERP.Modules.Users/
COPY ERP.Modules.Users.Contracts/ERP.Modules.Users.Contracts.csproj ERP.Modules.Users.Contracts/
COPY ERP.Modules.Wms/ERP.Modules.Wms.csproj ERP.Modules.Wms/
COPY ERP.Modules.Wms.Contracts/ERP.Modules.Wms.Contracts.csproj ERP.Modules.Wms.Contracts/
COPY ERP.Persistence/ERP.Persistence.csproj ERP.Persistence/
COPY ERP.Shared/ERP.Shared.csproj ERP.Shared/


# Restore dependencies
RUN dotnet restore ERP.Api/ERP.Api.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish ERP.Api/ERP.Api.csproj -c Release --no-restore -o /out /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ERP.Api.dll"]
