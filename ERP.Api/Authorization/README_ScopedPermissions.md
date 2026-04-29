# Atributos de Autorización con Scope Explícito

## Uso de los nuevos atributos

Se han implementado tres nuevos atributos para autorización con scope explícito:

### 1. RequireCompanyPermission
Requiere que el usuario tenga el permiso específico a nivel de compañía actual.

```csharp
[RequireCompanyPermission("Inventory.Stock.Read")]
[HttpGet("products")]
public async Task<IActionResult> GetProducts()
{
    // Solo usuarios con permiso de lectura de stock a nivel de compañía pueden acceder
}
```

### 2. RequireOrganizationPermission  
Requiere que el usuario tenga el permiso específico a nivel de organización.

```csharp
[RequireOrganizationPermission("Admin.Users.Read")]
[HttpGet("users")]
public async Task<IActionResult> GetUsers()
{
    // Solo usuarios con permiso de lectura de usuarios a nivel de organización pueden acceder
}
```

### 3. RequirePlatformPermission
Requiere que el usuario tenga el permiso específico a nivel de plataforma.

```csharp
[RequirePlatformPermission("Platform.Features.Manage")]
[HttpPost("features/{featureId}/enable")]
public async Task<IActionResult> EnableFeature(int featureId)
{
    // Solo usuarios con permiso de gestión de features a nivel de plataforma pueden acceder
}
```

## Compatibilidad hacia atrás

El atributo existente `[RequireCompanyPermission]` sigue funcionando:

```csharp
[RequireCompanyPermission(PermissionKeys.Accounting.ChartOfAccounts.Read)]
[HttpGet("chart-of-accounts")]
public async Task<IActionResult> GetChartOfAccounts()
{
    // Funciona exactamente como antes
}
```

## Validación de Scope

Los nuevos atributos no solo verifican que el usuario tenga el permiso, sino también que esté en el contexto adecuado:

- **Company**: Requiere `CompanyId > 0`, `CompanyPublicId` válido Y membresía activa en `CompanyUsers` (componente operational principal)
- **Organization**: Requiere `OrganizationId > 0`  
- **Platform**: Siempre válido si el usuario tiene el permiso

## Combinación con otros atributos

Puedes combinar los atributos de scope con otros atributos de autorización:

```csharp
[Authorize]
[RequireCompanyPermission("Sales.Orders.Create")]
[TenantGuard]
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // Requiere: autenticación + permiso de compañía + validación de tenant
}
```

## Integración con el sistema RBAC existente

Los nuevos atributos utilizan el mismo `IRbacService` que el sistema existente, por lo que:

- Los permisos se cachean igual (2 minutos)
- Se respetan las features habilitadas por compañía  
- Funciona con la misma lógica de multi-tenancy
- No se modifica la fuente de verdad de roles