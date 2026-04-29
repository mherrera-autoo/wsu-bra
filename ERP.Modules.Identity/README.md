# ERP.Modules.Identity

## Propósito del módulo

El módulo **Identity** es el **núcleo de autenticación, autorización y control de acceso** del ERP.

Define:
- Quién es un usuario
- En qué contexto puede operar (platform / tenant)
- A qué organizaciones y compañías tiene acceso
- Qué puede hacer dentro de cada compañía (RBAC)
- Cómo se emiten y validan los tokens

Este módulo **no contiene lógica de negocio**.  
Contiene **reglas de seguridad** que gobiernan todo el sistema.

---

## Regla mental (IAM)

- **OrganizationMember** → quién eres en el workspace
- **CompanyUser** → en qué empresa puedes operar
- **scope = platform** → aún no estás dentro de una empresa
- **scope = tenant** → ya estás operando dentro de una empresa (**companyPublicId + companyId**)

---

## Schema IAM

Todas las tablas IAM viven en el schema PostgreSQL **`iam`**.  
Las tablas de negocio permanecen en `public`.

---

## Conceptos clave

### User
Representa una identidad global del sistema.
- Es transversal a todas las organizaciones y compañías.
- No tiene acceso operativo por sí solo.

Tablas:
- `iam.Users`
- `iam.UserSessions`

---

### Organization
Representa el **workspace** lógico.
- Puede ser `Individual`, `Holding` o `MultiCompany`.
- Agrupa compañías y entidades tributarias.
- No es un tenant operativo.

Tabla:
- `iam.Organizations`

---

### Company (Tenant)
Es el **límite real de operación del sistema**.
- Toda operación de negocio ocurre dentro de una Company.
- Todos los datos de negocio están particionados por `CompanyId`.
- IAM referencia compañías por `CompanyPublicId` (UUID), sin FK cross-schema.

Tabla:
- `Companies`

---

## Membresías y acceso

### OrganizationMember
Define **quién eres dentro del workspace**.

Usos:
- Onboarding
- Administración global
- Gestión de compañías del holding

⚠️ **NO otorga acceso operativo a una compañía**

Tabla:
- `iam.OrganizationMembers`

---

### CompanyUser (fuente de verdad del acceso)
Define **en qué empresa puedes operar**.

Reglas:
- El acceso tenant depende **exclusivamente** de `CompanyUser`
- El usuario debe existir con `Status = Active`

Estados:
- `Active`
- `Suspended`
- `Disabled`

Tabla:
- `iam.CompanyUsers`

> **Principio crítico**  
> Sin `CompanyUser (Active)` **no hay acceso**, sin excepciones.

---

### CompanyLink (MultiCompany)
Puente explícito entre Organization y Company.

Uso:
- Restringir qué compañías puede seleccionar una organización tipo MultiCompany.
- Se valida **además** de `CompanyUser` cuando aplica.

Tabla:
- `iam.CompanyLinks`

---

### UserProfile (global)
Perfil global del usuario (no depende de Company).

Tabla:
- `iam.UserProfiles`

---

## Tokens (JWT)

El sistema utiliza **UN SOLO TIPO DE JWT**, con contexto explícito vía claims.

### Claims estándar
- `userId`
- `organizationId`
- `scope`
- `companyPublicId` (solo si scope = tenant)
- `companyId` (solo si scope = tenant)
- `roles`
- `permissions`

---

### scope = platform
Indica que el usuario **no está operando dentro de una empresa**.

Permite:
- Onboarding
- Selector de empresas
- Administración del workspace

Restricciones:
- ❌ No puede acceder a endpoints tenant
- ❌ No puede ejecutar lógica de negocio

---

### scope = tenant
Indica que el usuario **está operando dentro de una empresa específica**.

Requiere:
- `companyPublicId` + `companyId` válidos en el token
- `CompanyUser (Active)`

Permite:
- Todas las operaciones de negocio

---

## Guards / Policies

### TenantGuard
Todos los endpoints tenant deben pasar por este guard.

Reglas:
- `scope == tenant`
- `companyPublicId` presente en el token
- `companyId > 0` en el token
- consistencia `companyPublicId` ↔ `companyId`

❌ Nunca desde route / body / query.

---

### Platform endpoints
- Aceptan `scope = platform`
- No requieren `companyId`

---

## AccessService

### CanAccessCompanyAsync

Regla única de acceso:

```text
Existe CompanyUser(companyPublicId, userId) con Status = Active
```
