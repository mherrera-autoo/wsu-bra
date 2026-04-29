# Module Boundaries & Dependency Rules

## Reglas (no negociables)
1. Ningún módulo puede referenciar el Domain/Application/Infrastructure de otro módulo.
2. La única dependencia permitida entre módulos es vía `ERP.Modules.<X>.Contracts` (DTOs, interfaces, eventos).
3. Comunicación por defecto entre módulos: eventos de integración publicados a la outbox.
4. Contratos síncronos (interfaces) solo para queries/validaciones que requieren respuesta inmediata; no para ejecutar lógica del otro módulo.
5. `ERP.Api` y `ERP.Workers` son composition root: solo wiring de DI, sin lógica de negocio.
6. No se introducen dependencias técnicas nuevas (colas externas, Redis, etc.).

## Ejemplos reales en el código

### 1) Sales → Inventory (evento outbox)
- **Publicación**: `SalesService` publica `sales.shipment.requested` en outbox al aprobar órdenes/invoices.
- **Contrato**: `SalesShipmentRequested` vive en `ERP.Modules.Sales.Contracts`.
- **Consumo**: `SalesShipmentRequestedHandler` en Inventory aplica el movimiento dentro de transacción propia.

### 2) Purchasing → Inventory (evento outbox)
- **Publicación**: `PurchasingService` publica `purchasing.goods-receipt.requested` en outbox al recibir mercadería.
- **Contrato**: `GoodsReceiptRequested` vive en `ERP.Modules.Purchasing.Contracts`.
- **Consumo**: `GoodsReceiptRequestedHandler` en Inventory aplica el ingreso.

### 3) Sales → Accounting (evento outbox)
- **Publicación**: `SalesService` publica `sales.receivable.requested` al aprobar invoices/credit notes.
- **Contrato**: `SalesReceivableRequested` vive en `ERP.Modules.Sales.Contracts`.
- **Consumo**: `SalesReceivableRequestedHandler` en Accounting crea el receivable.

## Notas de implementación
- Los eventos se serializan como JSON y se guardan en `OutboxMessages` en la misma transacción del módulo emisor.
- `ERP.Workers` procesa la outbox y enruta mensajes a los handlers registrados (`IOutboxMessageHandler`).
