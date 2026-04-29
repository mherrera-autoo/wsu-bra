# Flujo de Cotizacion de Venta (SalesQuote)

## Objetivo

Implementar el primer documento economico del dominio Comercial:

- **Cotizacion de Venta (`SalesQuote`)** como documento de intencion.
- Flujo de estados: **Draft -> Submitted -> Approved / Rejected**.
- Publicacion de eventos para reaccion de otros modulos (Workflow, Documents y Accounting cuando corresponde).

## Alcance funcional

- La cotizacion representa una **intencion comercial**, no una ejecucion operativa.
- **No genera salida de stock** (`stock-out`).
- Calcula montos economicos del documento:
  - `Subtotal`
  - `TaxTotal`
  - `Total`
- Soporta lineas con `ProductId` opcional, descripcion, cantidad, precio unitario y tasa de impuesto opcional.

## Modelo y reglas de dominio

Entidad principal: `SalesQuote`.

Estados:

- `Draft`
- `Submitted`
- `Approved`
- `Rejected`
- `Cancelled` (definido en el enum para evolucion futura)

Reglas de transicion:

- Solo `Draft` puede:
  - editar lineas
  - pasar a `Submitted`
- Solo `Submitted` puede:
  - pasar a `Approved`
  - pasar a `Rejected`
- Si se intenta una transicion invalida, se responde conflicto de estado.

Validaciones de negocio principales:

- `CustomerName` obligatorio.
- Debe existir al menos una linea.
- `Quantity > 0`.
- `UnitPrice >= 0`.
- `TaxRate >= 0` cuando se informa.
- En rechazo, `Reason` obligatorio.

## API expuesta

Controlador: `api/sales/quotes`.

Operaciones:

- `POST /api/sales/quotes` crea cotizacion en `Draft`.
- `GET /api/sales/quotes/{id}` obtiene detalle por `PublicId`.
- `GET /api/sales/quotes` busca/pagina por filtros (`status`, `q`, rango fecha, `page`, `pageSize`).
- `POST /api/sales/quotes/{id}/submit` cambia a `Submitted`.
- `POST /api/sales/quotes/{id}/approve` cambia a `Approved`.
- `POST /api/sales/quotes/{id}/reject` cambia a `Rejected`.

Permisos:

- Lectura: `Sales.Quotes.Read`
- Escritura/creacion/submit: `Sales.Quotes.Write`
- Aprobacion/rechazo: `Sales.Quotes.Approve`

## Eventos publicados (Outbox)

Cuando la cotizacion cambia de estado, se publica en outbox:

- `sales.quote.created`
- `sales.quote.submitted`
- `sales.quote.approved`
- `sales.quote.rejected`

Esto permite desacoplar efectos secundarios y mantener consistencia transaccional (persistencia + evento en la misma unidad de trabajo).

## Reaccion por modulo

### 1) Workflow (al enviar)

Evento consumido:

- `sales.quote.submitted`

Accion:

- Inicia workflow para documento tipo `SalesQuote`.
- Si ya existe workflow o no hay definicion activa, no rompe el flujo principal (manejo tolerante).

### 2) Documents (al aprobar)

Evento consumido:

- `sales.quote.approved`

Accion:

- Crea un documento asociado a la cotizacion aprobada.
- Publica `documents.linked` para dejar trazabilidad del vinculo.

### 3) Accounting (al aprobar, si corresponde)

Evento consumido:

- `sales.quote.approved`

Accion:

- Intenta posteo contable con `AccountingPostingService`.
- Solo ejecuta si la empresa tiene configuradas cuentas contables minimas:
  - `AccountsReceivableAccountId`
  - `SalesRevenueAccountId`
- Asiento generado:
  - Debe: Cuentas por cobrar por `quote.Total`
  - Haber: Ingreso por ventas por `quote.Total`

Si la configuracion no existe, se omite el posteo sin fallar la aprobacion comercial.

## Consideraciones de multi-tenant

- Todas las operaciones se filtran por `CompanyId` desde `ITenantContext`.
- En API el contexto viene del request autenticado.
- En workers (sin `HttpContext`) se usa `MutableTenantContext` para setear empresa/scope antes de operaciones que requieren tenant (ejemplo: posteo contable).

## Flujo resumido

1. Usuario crea cotizacion -> queda `Draft` y publica `sales.quote.created`.
2. Usuario envia cotizacion -> `Submitted` y publica `sales.quote.submitted`.
3. Workflow reacciona e inicia proceso de aprobacion documental.
4. Usuario aprueba o rechaza:
   - Si aprueba -> `Approved`, publica `sales.quote.approved`, se dispara Documents y Accounting (si hay configuracion).
   - Si rechaza -> `Rejected`, publica `sales.quote.rejected`.

## Resultado contra el objetivo inicial

Se cumple el objetivo definido:

- Existe `SalesQuote` como primer documento economico comercial.
- Se implemento el flujo `Draft -> Submitted -> Approved / Rejected`.
- Se publican eventos de dominio para integracion con modulos externos.
- El flujo mantiene naturaleza de intencion comercial: **sin impacto en stock fisico**.
