namespace ERP.Shared.Application;

public static class RoleNames
{
    // Platform scope (SaaS operator)
    public const string PlatformSuperAdmin = "PlatformSuperAdmin";
    public const string PlatformSupport = "PlatformSupport";
    public const string PlatformBilling = "PlatformBilling";
    public const string PlatformAuditor = "PlatformAuditor";

    // Organization scope (workspace)
    public const string OrganizationOwner = "OrganizationOwner";

    // Platform delegated ops (si ya tienes FeaturePackAdder, mantenlo)
    public const string FeaturePackAdder = "FeaturePackAdder"; // puede habilitar packs/flags (delegado del superadmin)



    // Company scope (tenant)
    public const string OperationsManager = "OperationsManager"; // gerente de operaciones
    public const string PharmaceuticalChemist = "PharmaceuticalChemist";
    // Commercial / pricing
    public const string CommercialManager = "CommercialManager";

    public const string CompanyOwner = "CompanyOwner";           // opcional: “dueño” del tenant
    public const string CompanyAdmin = "CompanyAdmin";           // ya existe

    // Accounting / Finance
    public const string CompanyAccountant = "CompanyAccountant"; // contabilidad (libros, periodos, cierres)
    public const string FinanceManager = "FinanceManager";       // tesorería/AR/AP
    public const string FinanceClerk = "FinanceClerk";           // registra pagos/cobros
    public const string BillingClerk = "BillingClerk";           // emite/gestiona docs venta/compra (MVP)

    // Sales / Purchasing
    public const string SalesClerk = "SalesClerk";
    public const string PurchasingClerk = "PurchasingClerk";

    // Inventory / Warehouse
    public const string WarehouseManager = "WarehouseManager";   // ajustes/mermas
    public const string WarehouseClerk = "WarehouseClerk";       // recepciones/despachos/transfer

    // Read-only
    public const string CompanyAuditor = "CompanyAuditor";       // lectura transversal

    // Pharmacy (solo si pack)
    public const string PharmaceuticalManager = "PharmaceuticalManager";
    public const string Pharmacist = "Pharmacist";
    public const string PharmacySupervisor = "PharmacySupervisor";
    public const string PharmacyAuditor = "PharmacyAuditor";
    public const string RfidManager = "RfidManager";

    // Technical / system (no visibles)
    public const string SystemEventProcessor = "SystemEventProcessor"; // ERP.Workers
}
