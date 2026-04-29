# Modern & Fast ERP (.NET 10) — Scaffold

## Core value proposition
- Built around **real stock and warehouse management**, enabling **fast, low-friction business flows**.
- **Inventory is not a price list.** Inventory is modeled independently from pricing.
- **Inventory ≠ Stock ≠ Pricing**
  - Inventory = products (structure)
  - Stock = quantities per warehouse (reality)
  - Pricing = sales strategy (business)

## Design principles
- Stock is the result of movements, not manual updates.
- Warehouses are first-class entities, not just filters.
- Purchasing creates intent. Receiving creates stock.
- Customers & suppliers are business entities, not accounting auxiliaries.

## Solution layout
- ERP.Api (HTTP entrypoint)
- ERP.Workers (background jobs / outbox placeholder)
- ERP.Shared (shared primitives)
- ERP.Modules.* (bounded contexts; Domain/Application/Infrastructure folders)

> Targets net10.0 by request. Requires a .NET 10 SDK installed locally to build.

## Local setup (PostgreSQL + migrations)

### Configuration files
The API reads settings from `ERP.Api/appsettings.json` plus environment-specific overrides:
- `ERP.Api/appsettings.Development.json`
- `ERP.Api/appsettings.Production.json`

Set `ASPNETCORE_ENVIRONMENT` to `Development` or `Production` and fill in the
corresponding values (or supply them via environment variables).

### WSU feature flags
- `Wsu:ValidarOperadorEnOrden` (bool, default `false`): when `true`, `POST /api/wsu/orders/{publicId}/confirm` enforces RFID operator enrollment (active credential with both `FaceTemplateId` and `NfcCardUid`); when `false`, this validation is skipped.

### 1) Start PostgreSQL
```bash
docker compose up -d

OPTIONAL, reset docker: docker compose down -v
```



Default connection string:
```
Host=localhost;Port=5432;Database=erp;Username=erp;Password=erp
```

You can override it with the `ERP_DB` environment variable or `ConnectionStrings:ErpDatabase`.

### 2) Apply migrations
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations list -p ERP.Persistence -s ERP.Api -c ERP.Persistence.ErpDbContext
dotnet ef database update -p ERP.Persistence -s ERP.Api
```

#### Troubleshooting migrations
If `dotnet ef database update` fails with `relation "<table>" already exists`, the database
has tables that are not recorded in `__EFMigrationsHistory`. For local development, reset
the database and re-apply migrations:

```bash
docker compose down -v
docker compose up -d

delete all content inside Erp.Persistence.Migrations

dotnet ef migrations add Initial -p ERP.Persistence -s ERP.Api
dotnet ef database update -p ERP.Persistence -s ERP.Api
```

If you need to keep existing data, either remove the conflicting tables or insert the
missing migration row into `__EFMigrationsHistory` before rerunning the update.

### 3) Run the API and workers
```bash
dotnet run --project ERP.Api
dotnet run --project ERP.Workers
```

## Development demo seed
In Development only, you can seed fictional Chilean demo data.
Run these endpoints in order. All use `X-DEV-SEED-KEY`.

```bash
curl -X POST "http://localhost:5000/api/dev/seed/base" -H "X-DEV-SEED-KEY: <your-dev-seed-key>"
```

```bash
curl -X POST "http://localhost:5000/api/dev/seed/accounting" -H "X-DEV-SEED-KEY: <your-dev-seed-key>"
```

```bash
curl -X POST "http://localhost:5000/api/dev/seed/prices" -H "X-DEV-SEED-KEY: <your-dev-seed-key>"
```

```bash
curl -X POST "http://localhost:5000/api/dev/seed/operation" -H "X-DEV-SEED-KEY: <your-dev-seed-key>"
```

To wipe previous data for a stage and reseed it:

```bash
curl -X POST "http://localhost:5000/api/dev/seed/base?force=true" -H "X-DEV-SEED-KEY: <your-dev-seed-key>"
```

These endpoints are not mapped outside Development, so they return 404 in Production.

## Initial seed (global)
Run these endpoints in order. All are idempotent and use `X-GLOBAL-SEED-KEY`.

1) Global catalog (`/api/seed/global`): migrations, global units of measure, and geo catalog.

```bash
curl -X POST "http://localhost:5000/api/seed/global" -H "X-GLOBAL-SEED-KEY: <your-global-seed-key>"
```

2) Features (`/api/seed/features`): global feature catalog.

```bash
curl -X POST "http://localhost:5000/api/seed/features" -H "X-GLOBAL-SEED-KEY: <your-global-seed-key>"
```

3) RBAC base (`/api/seed/global/rbac`):
`Platform.Features.Manage`, `Platform.GlobalMenu.View`, `Platform.IAM.Manage`, `Platform.GEO.Manage`,
`Admin.Users.Read`, `Admin.Users.Write`, `Admin.Roles.Assign`,
`Workspace.Companies.Create`, `Workspace.Members.Invite`, `Workspace.Manage`,
`Company.Admin.Manage`, `Company.Warehouse.Manage`, `Company.Stock.Manage`.

```bash
curl -X POST "http://localhost:5000/api/seed/global/rbac" -H "X-GLOBAL-SEED-KEY: <your-global-seed-key>"
```

4) RBAC otros (`/api/seed/global/rbac/otros`): all remaining permissions and role-permission mappings.

```bash
curl -X POST "http://localhost:5000/api/seed/global/rbac/otros" -H "X-GLOBAL-SEED-KEY: <your-global-seed-key>"
```

5) Global accounting (`/api/seed/global/accounting`): chart of accounts template and global currencies.

```bash
curl -X POST "http://localhost:5000/api/seed/global/accounting" -H "X-GLOBAL-SEED-KEY: <your-global-seed-key>"
```

Provide the key via `GlobalSeed:Key` (for `/api/seed/global`, `/api/seed/features`, `/api/seed/global/rbac`, `/api/seed/global/rbac/otros`, `/api/seed/global/accounting`, and `/api/seed/rfid`, sent as `X-GLOBAL-SEED-KEY`) and `DevSeed:Key` (for `/api/dev/seed/base`, `/api/dev/seed/accounting`, `/api/dev/seed/prices`, and `/api/dev/seed/operation`, sent as `X-DEV-SEED-KEY`).

# Biblia de arquitectura — ERP SaaS  
Modular (.NET 10 + PostgreSQL + SPA)

## 0) Objetivo

ERP SaaS multi-tenant (muchas empresas) con flujo rápido, modelo de dominio limpio y extensiones verticales (packs) sin ensuciar el core.  
Enfoque operativo: stock y bodegas como first-class domains, desacoplado de pricing y contabilidad legacy.

## 1) Principios no negociables

### 1.1 Multi-tenant por JWT (CompanyId)

CompanyId SIEMPRE se resuelve desde el JWT (tenant context).  
CompanyId NUNCA se acepta como parámetro (route / query / body).  
Toda query / command debe filtrar por CompanyId desde ITenantContext.

### 1.2 Modularidad fuerte (ERP.Modules.*)

Módulos independientes: cada módulo tiene Domain / Application / Persistence (DDD light).  
Evitar dependencias cruzadas directas → integración event-driven.  
ERP.Shared contiene contratos, primitives, Result, UnitOfWork y eventos compartidos.

### 1.3 Event-driven + Outbox

Publicación de eventos con Outbox en la misma transacción.  
Módulos reaccionan vía handlers / subscribers.  
El core no referencia extensiones (ej: Inventory no referencia PharmaceuticalRegulatedInventory).

### 1.4 “Append-only” donde corresponde

Contabilidad: posted immutable; correcciones por reversa (nuevo asiento).  
Compliance / Libros oficiales: append-only (sin UPDATE / DELETE).

### 1.5 Inventario “físico”

Inventario NO es pricing.  
Stock es por bodega.  
No se “edita stock”; el stock es resultado de movimientos.

## 2) Arquitectura general de solución (.NET 10)

ERP.Api (host): controllers mínimos, auth, DI, swagger, middleware, health, dev seed.  

ERP.Shared:
- ITenantContext  
- IUnitOfWork  
- Result  
- Integration event contracts  
- Outbox abstractions  

ERP.Persistence:
- DbContext  
- Migrations  

ERP.Modules.*  
Identity, MasterData, Inventory, Purchasing, Sales, Accounting, PharmaceuticalRegulatedInventory, etc.

## 3) Identidad, usuarios y multi-empresa

### 3.1 Users vs perfiles vs empresas

Users = credenciales base  
Id, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt  

UserProfiles = datos del perfil  

Para multi-empresa real usar CompanyUsers (UserId + CompanyId + Roles + Estado).  
CompanyId en Users solo sirve si 1 usuario = 1 empresa (limitante).

### 3.2 deviceInfo en login

email/password ingresados por formulario.  
deviceInfo.id = UUID persistido en localStorage.  
deviceInfo.name = “Chrome - Windows”.  
deviceInfo.userAgent = navigator.userAgent.  

Permite sesiones por dispositivo + logout granular.

## 4) Gobernanza de packs / feature flags

### 4.1 Modelo

CompanyFeatures  
CompanyId  
FeatureKey  

### 4.2 Quién puede asignar packs

Solo PlatformSuperAdmin.  
CompanyAdmin NO puede habilitar packs.  
Opción futura: FeatureManager delegable.

### 4.3 Auditoría obligatoria

CompanyFeatureAudit  
CompanyId  
FeatureKey  
Action  
ChangedAt  
ChangedByUserId  
Reason  
CorrelationId  

### 4.4 Servicio único

IFeatureService.IsEnabled(companyId, featureKey) con cache + invalidación.  
Aplicado en endpoints, servicios y event handlers.

## 5) Producto — Capabilities over Types

No usar enum Tipo (Producto / Insumo / Servicio / Activo).  
Usar capabilities.

### 5.1 MVP Product

Sku  
Name  
UnitOfMeasureId  
IsStockable  

### 5.2 Evolución (PR1)

IsSellable  
IsPurchasable  

Regla:  
Si IsStockable = true entonces IsSellable o IsPurchasable debe ser true.  

Servicios:  
IsStockable = false  
IsSellable = true  
IsPurchasable = false  

Activos fijos viven en FixedAssets.

### 5.3 Qué NO va en Product

IVA / impuestos → Tax  
Cuentas contables → Accounting profiles  
Series / lotes → Inventory / PharmaceuticalRegulatedInventory  

## 6) Inventario — stock y bodegas

Stock = (ProductId, WarehouseId).  
Toda variación es un movimiento: IN, OUT, TRANSFER, ADJUST.

Modelo:
- Warehouse  
- StockBalance  
- InventoryMovement  

Siempre publicar StockMoved.

## 7) Compras y Ventas

Compras:  
PurchaseOrder  
GoodsReceipt → genera Stock IN  

Ventas:  
Documentos y órdenes → Stock OUT  

## 8) PharmaceuticalRegulatedInventory como extensión

Packs:
- PharmaceuticalRegulatedInventory.Base  
- TODO: ERP.Modules.RetailPharmacy.Dispensing  
- PharmaceuticalRegulatedInventory.ComplianceISP  

Inventory no referencia PharmaceuticalRegulatedInventory.

### 8.1 PharmaceuticalRegulatedInventory.Base

PharmaProductProfile  
ProductId  
RequiresBatch  
RequiresExpiry  
SaleCondition  
IsRegulated  

StockBatch  
ProductId  
WarehouseId  
BatchNumber  
ExpiryDate  
QuantityOnHand  

MovementBatchAllocation  
MovementId  
StockBatchId  
Quantity  

Reglas: batch y expiry obligatorios si aplica.  
Salida por FEFO.

### 8.2 TODO: ERP.Modules.RetailPharmacy.Dispensing

Mover Dispense/Prescription a ERP.Modules.RetailPharmacy (dispensación/POS/recetas).

Dispense + DispenseLine.  
Opcional Prescription.  
Confirmar genera Stock OUT respetando FEFO y batch.  

### 8.3 PharmaceuticalRegulatedInventory.ComplianceISP

OfficialControlledBookEntry (append-only).  
Escucha StockMoved y DispenseConfirmed.  
Correcciones = nuevo registro.

## 9) Accounting Core

Double-entry.  
Append-only.  
Periodos cerrados no aceptan posting.  

Entidades:
- Account  
- Journal  
- AccountingPeriod  
- JournalEntry  
- JournalEntryLine  
- CompanyAccountingSettings  

Idempotencia:
(CompanyId, SourceModule, SourceDocumentId, SourceDocumentType)

Posting por eventos: AccountingPostRequested → JournalEntryPosted.  
Reversa vía AccountingReverseRequested.

## 10) UI schemas dinámicos

GET /api/ui-schemas/{screen}  
CompanyId siempre desde JWT.  

Screens:
- product-editor  
- inventory-receipt-line  
- dispense-form  

Campos visibles y requeridos según packs.

## 11) Frontend

SPA privada (CSR).  
Vite + React + TypeScript + Router + TanStack Query.  

Menú:
- Finanzas  
- Comercial  
- Operaciones  
- RRHH  
- Administración  

## 12) Dev seed

Endpoints solo en Development:
- `/api/dev/seed/base`: empresas demo, tax entity, moneda por empresa, UoM, bodegas, productos, clientes y proveedores.
- `/api/dev/seed/accounting`: journals, periodos, settings y cuentas base.
- `/api/dev/seed/prices`: lista de precios y precios demo.
- `/api/dev/seed/operation`: compras, recepciones y ventas demo.

## 13) Filosofía anti-ERP legacy

No mezclar operación con contabilidad.  
No acoplar inventario a pricing.  
No auxiliares genéricos.  
Extensiones por packs.  
Todo multi-tenant con CompanyId desde JWT.
