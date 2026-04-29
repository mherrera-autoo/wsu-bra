namespace ERP.Shared.Application;

public static class PermissionKeys
{
    public static class Admin
    {
        public const string UsersRead = "Admin.Users.Read";
        public const string UsersWrite = "Admin.Users.Write";
        public const string RolesAssign = "Admin.Roles.Assign";
    }

    public static class Accounting
    {
        public const string ChartOfAccountsRead = "Accounting.ChartOfAccounts.Read";
        public const string ChartOfAccountsWrite = "Accounting.ChartOfAccounts.Write";
        public const string JournalRead = "Accounting.Journal.Read";
        public const string JournalAdjustmentsWrite = "Accounting.Journal.Adjustments.Write";
        public const string PeriodClose = "Accounting.Period.Close";
        public const string TaxComplianceRead = "Accounting.TaxCompliance.Read";
        public const string TaxComplianceWrite = "Accounting.TaxCompliance.Write";
        public const string AutomationRulesManage = "Accounting.AutomationRules.Manage";
    }

    public static class Documents
    {
        public const string Read = "Documents.Read";
        public const string Write = "Documents.Write";
        public const string Upload = "Documents.Upload";
        public const string VersionAdd = "Documents.Version.Add";
        public const string Link = "Documents.Link";
        public const string Archive = "Documents.Archive";
    }

    public static class Inventory
    {
        public const string StockRead = "Inventory.Stock.Read";
        public const string MovementsCreate = "Inventory.Movements.Create";
        public const string AdjustmentsCreate = "Inventory.Adjustments.Create";
    }

    public static class Platform
    {
        public const string FeaturesManage = "Platform.Features.Manage";
        public const string GlobalMenuView = "Platform.GlobalMenu.View";
        public const string GlobalIAMManager = "Platform.IAM.Manage";
        public const string GlobalGEOManager = "Platform.GEO.Manage";
        public const string WsuManage = "Platform.WSU.Manage";
        public const string WsuEventsConsumption = "Platform.WSU.EventsConsumption";
        public const string WsuEventsIngest = "Platform.WSU.EventsIngest";
    }

    public static class Pricing
    {
        public const string MarginWrite = "Pricing.Margin.Write";
    }

    public static class Sales
    {
        public const string QuotesRead = "Sales.Quotes.Read";
        public const string QuotesWrite = "Sales.Quotes.Write";
        public const string QuotesApprove = "Sales.Quotes.Approve";
    }

    public static class Pharmaceutical
    {
        public const string PurchaseSuggestionsRead = "Pharmaceutical.PurchaseSuggestions.Read";
        public const string PurchaseSuggestionsCreate = "Pharmaceutical.PurchaseSuggestions.Create";
        public const string PurchaseSuggestionsUpdate = "Pharmaceutical.PurchaseSuggestions.Update";
        public const string PurchaseSuggestionsDelete = "Pharmaceutical.PurchaseSuggestions.Delete";
        public const string PurchaseSuggestionsExport = "Pharmaceutical.PurchaseSuggestions.Export";

        public const string PurchaseOrdersRead = "Pharmaceutical.PurchaseOrders.Read";
        public const string PurchaseOrdersCreate = "Pharmaceutical.PurchaseOrders.Create";
        public const string PurchaseOrdersUpdate = "Pharmaceutical.PurchaseOrders.Update";
        public const string PurchaseOrdersDelete = "Pharmaceutical.PurchaseOrders.Delete";
        public const string PurchaseOrdersSubmit = "Pharmaceutical.PurchaseOrders.Submit";
        public const string PurchaseOrdersApprove = "Pharmaceutical.PurchaseOrders.Approve";
        public const string PurchaseOrdersReject = "Pharmaceutical.PurchaseOrders.Reject";
        public const string PurchaseOrdersCancel = "Pharmaceutical.PurchaseOrders.Cancel";
        public const string PurchaseOrdersExport = "Pharmaceutical.PurchaseOrders.Export";

        public const string GoodsReceiptsRead = "Pharmaceutical.GoodsReceipts.Read";
        public const string GoodsReceiptsCreate = "Pharmaceutical.GoodsReceipts.Create";
        public const string GoodsReceiptsUpdate = "Pharmaceutical.GoodsReceipts.Update";
        public const string GoodsReceiptsDelete = "Pharmaceutical.GoodsReceipts.Delete";
        public const string GoodsReceiptsConfirm = "Pharmaceutical.GoodsReceipts.Confirm";
        public const string GoodsReceiptsCancel = "Pharmaceutical.GoodsReceipts.Cancel";
        public const string GoodsReceiptsExport = "Pharmaceutical.GoodsReceipts.Export";

        public const string InventoryRead = "Pharmaceutical.Inventory.Read";
        public const string BatchesRead = "Pharmaceutical.Batches.Read";
        public const string BatchesCreate = "Pharmaceutical.Batches.Create";
        public const string BatchesUpdate = "Pharmaceutical.Batches.Update";
        public const string BatchesAdjust = "Pharmaceutical.Batches.Adjust";
        public const string BatchesDelete = "Pharmaceutical.Batches.Delete";
        public const string BatchesExport = "Pharmaceutical.Batches.Export";

        public const string TraceabilityRead = "Pharmaceutical.Traceability.Read";
        public const string TraceabilityExport = "Pharmaceutical.Traceability.Export";

        public const string AlertsRead = "Pharmaceutical.Alerts.Read";
        public const string AlertsResolve = "Pharmaceutical.Alerts.Resolve";
        public const string AlertsExport = "Pharmaceutical.Alerts.Export";

        public const string ReportsRead = "Pharmaceutical.Reports.Read";
        public const string ReportsExport = "Pharmaceutical.Reports.Export";

        public const string ControlledBookRead = "Pharmaceutical.ControlledBook.Read";
        public const string ControlledBookAdjustmentsCreate = "Pharmaceutical.ControlledBook.Adjustments.Create";
        public const string ControlledBookEntriesCreate = "Pharmaceutical.ControlledBook.Entries.Create";
        public const string ControlledBookExitsCreate = "Pharmaceutical.ControlledBook.Exits.Create";
        public const string ControlledBookExport = "Pharmaceutical.ControlledBook.Export";

        public const string DispenseRead = "Pharmaceutical.Dispense.Read";
        public const string DispenseCreate = "Pharmaceutical.Dispense.Create";
    }

    public static class Workspace
    {
        public const string CompaniesCreate = "Workspace.Companies.Create";
        public const string MembersInvite = "Workspace.Members.Invite";
        public const string Manage = "Workspace.Manage";
    }

    public static class Company
    {
        public const string AdminManage = "Company.Admin.Manage";
        public const string WarehouseManage = "Company.Warehouse.Manage";
        public const string StockManage = "Company.Stock.Manage";
        public const string CostumersManage = "Company.Costumers.Manage";
        public const string CurrenciesManage = "Company.Currencies.Manage";
        public const string SuppliersManage = "Company.Suppliers.Manage";
    }

    public static class Rfid
    {
        public const string TagsManage = "Rfid.Tags.Manage";
        public const string AccessSessionsManage = "Rfid.AccessSessions.Manage";
        public const string EventsManage = "Rfid.Events.Manage";
        public const string MovementLinksManage = "Rfid.MovementLinks.Manage";
    }
}
