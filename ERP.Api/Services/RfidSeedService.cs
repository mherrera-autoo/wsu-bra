using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Api.Configuration;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Rfid.Domain;
using ERP.Persistence;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ERP.Api.Services;

public sealed class RfidSeedService
{
    private const string CompanyName = "WE STOCK YOU SPA";
    private const string CompanyTaxId = "77777777-7";
    private const string WarehouseCode = "BRA";
    private const string WarehouseName = "Bodega Remota Autónoma";
    private static readonly string[] SeedTagEpcs =
    [
        "300833B2DDD9014000000001",
        "300833B2DDD9014000000002",
        "300833B2DDD9014000000003",
        "300833B2DDD9014000000004",
        "300833B2DDD9014000000005",
        "300833B2DDD9014000000006",
        "300833B2DDD9014000000007",
        "300833B2DDD9014000000008",
        "300833B2DDD9014000000009",
        "300833B2DDD901400000000A",
        "300833B2DDD901400000000B",
        "300833B2DDD901400000000C",
        "300833B2DDD901400000000D",
        "300833B2DDD901400000000E",
        "300833B2DDD901400000000F",
        "300833B2DDD9014000000010",
        "300833B2DDD9014000000011",
        "300833B2DDD9014000000012",
        "300833B2DDD9014000000013",
        "300833B2DDD9014000000014",
        "300833B2DDD9014000000015",
        "300833B2DDD9014000000016",
        "300833B2DDD9014000000017",
        "300833B2DDD9014000000018",
        "300833B2DDD9014000000019",
        "300833B2DDD901400000001A",
        "300833B2DDD901400000001B",
        "300833B2DDD901400000001C",
        "300833B2DDD901400000001D",
        "300833B2DDD901400000001E",
        "300833B2DDD901400000001F",
        "300833B2DDD9014000000020",
        "300833B2DDD9014000000021",
        "300833B2DDD9014000000022",
        "300833B2DDD9014000000023",
        "300833B2DDD9014000000024",
        "300833B2DDD9014000000025",
        "300833B2DDD9014000000026",
        "300833B2DDD9014000000027",
        "300833B2DDD9014000000028",
        "300833B2DDD9014000000029",
        "300833B2DDD901400000002A",
        "300833B2DDD901400000002B",
        "300833B2DDD901400000002C",
        "300833B2DDD901400000002D",
        "300833B2DDD901400000002E",
        "300833B2DDD901400000002F",
        "300833B2DDD9014000000030",
        "300833B2DDD9014000000031",
        "300833B2DDD9014000000032"
    ];
    private const string EventId = "RFID-SEED-EVT-001";
    private const string SessionId = "RFID-SEED-SESSION-001";
    private const string DeviceCode = "RFID-GATE-BRA";
    private const string SeedOperatorName = "Operador RFID Seed";
    private const string SeedOperatorDocumentId = "11111111-1";
    private static readonly (string Sku, string Name, string Barcode)[] SeedProducts =
    [
        ("RFID-PROD-001", "Producto RFID 001", "780099000001"),
        ("RFID-PROD-002", "Producto RFID 002", "780099000002"),
        ("RFID-PROD-003", "Producto RFID 003", "780099000003"),
        ("RFID-PROD-004", "Producto RFID 004", "780099000004"),
        ("RFID-PROD-005", "Producto RFID 005", "780099000005"),
        ("RFID-PROD-006", "Producto RFID 006", "780099000006"),
        ("RFID-PROD-007", "Producto RFID 007", "780099000007"),
        ("RFID-PROD-008", "Producto RFID 008", "780099000008"),
        ("RFID-PROD-009", "Producto RFID 009", "780099000009"),
        ("RFID-PROD-010", "Producto RFID 010", "780099000010")
    ];
    private static readonly string[] RfidCompanyUnitCanonicalCodes = ["ea", "kg", "l", "m", "box", "pallet"];
    private readonly ErpDbContext _dbContext;
    private readonly WsuOptions _wsuOptions;

    public RfidSeedService(ErpDbContext dbContext, IOptions<WsuOptions> wsuOptions)
    {
        _dbContext = dbContext;
        _wsuOptions = wsuOptions.Value;
    }

    public async Task<RfidSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var organization = await EnsureOrganizationAsync(cancellationToken);
        var company = await EnsureCompanyAsync(organization, cancellationToken);
        var taxEntity = await EnsureTaxEntityAsync(company, cancellationToken);
        var warehouse = await EnsureWarehouseAsync(company, cancellationToken);

        var products = await EnsureProductsAsync(company.Id, cancellationToken);
        if (products.Count < SeedProducts.Length)
        {
            throw new InvalidOperationException($"RFID seed expected {SeedProducts.Length} products but found {products.Count}.");
        }

        var sampleProductPublicId = products[0].PublicId;

        var (firstTag, firstTagEpc) = await EnsureTagsAsync(company.PublicId, sampleProductPublicId, SeedTagEpcs, cancellationToken);
        var inboxEvent = await EnsureEventInboxAsync(company.PublicId, warehouse.PublicId, firstTagEpc, cancellationToken);
        var op = await EnsureOperatorAsync(company.PublicId, cancellationToken);
        var accessSession = await EnsureAccessSessionAsync(company.PublicId, warehouse.PublicId, op.Id, cancellationToken);
        var movementLink = await EnsureMovementLinkAsync(company.PublicId, sampleProductPublicId, firstTagEpc, cancellationToken);
        await EnsureFeatureCatalogAsync(cancellationToken);
        await EnsureRfidRoleAndPermissionsAsync(cancellationToken);

        return new RfidSeedResult(
            organization.Id,
            company.Id,
            taxEntity.Id,
            warehouse.Id,
            products.Count,
            firstTag.Id,
            inboxEvent.Id,
            accessSession.Id,
            movementLink.Id,
            "RFID seed completed.");
    }

    public async Task<RfidSeedUomResult> SeedMasterCompanyUomAsync(CancellationToken cancellationToken = default)
    {
        var masterCompanyName = _wsuOptions.MasterCompanyName?.Trim();
        if (string.IsNullOrWhiteSpace(masterCompanyName))
        {
            throw new InvalidOperationException("Wsu:MasterCompanyName is not configured.");
        }

        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(item => item.Name == masterCompanyName, cancellationToken);
        if (company is null)
        {
            throw new InvalidOperationException($"Company '{masterCompanyName}' was not found.");
        }

        var requiredCodes = RfidCompanyUnitCanonicalCodes;
        var units = await _dbContext.UnitOfMeasures
            .Where(item => requiredCodes.Contains(item.CanonicalCode))
            .ToListAsync(cancellationToken);
        var unitsByCode = units.ToDictionary(item => item.CanonicalCode, StringComparer.OrdinalIgnoreCase);

        var missingCodes = requiredCodes
            .Where(code => !unitsByCode.ContainsKey(code))
            .ToArray();
        if (missingCodes.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing global units of measure: {string.Join(", ", missingCodes)}. Run /api/seed/global first.");
        }

        var unitIds = units.Select(item => item.Id).ToArray();
        var existingCompanyUnits = await _dbContext.CompanyUnitsOfMeasure
            .Where(item => item.CompanyId == company.Id && unitIds.Contains(item.UnitOfMeasureId))
            .ToListAsync(cancellationToken);
        var companyUnitsByUnitId = existingCompanyUnits.ToDictionary(item => item.UnitOfMeasureId);

        var unitResults = new List<RfidSeedUomItemResult>(requiredCodes.Length);
        var companyUnitsByCode = new Dictionary<string, CompanyUnitOfMeasure>(StringComparer.OrdinalIgnoreCase);

        foreach (var code in requiredCodes)
        {
            var unit = unitsByCode[code];
            if (companyUnitsByUnitId.TryGetValue(unit.Id, out var companyUnit))
            {
                if (companyUnit.IsEnabled)
                {
                    unitResults.Add(new RfidSeedUomItemResult(code, unit.Id, "already_enabled"));
                }
                else
                {
                    companyUnit.Enable(companyUnit.DisplayNameOverride, companyUnit.SortOrder);
                    unitResults.Add(new RfidSeedUomItemResult(code, unit.Id, "enabled"));
                }

                companyUnitsByCode[code] = companyUnit;
                continue;
            }

            var created = CompanyUnitOfMeasure.Create(
                company.Id,
                unit.Id,
                unit.Dimension,
                displayNameOverride: null,
                sortOrder: null,
                isEnabled: true,
                isDefaultForDimension: false);

            await _dbContext.CompanyUnitsOfMeasure.AddAsync(created, cancellationToken);
            companyUnitsByUnitId[unit.Id] = created;
            companyUnitsByCode[code] = created;
            unitResults.Add(new RfidSeedUomItemResult(code, unit.Id, "created"));
        }

        var defaultTargets = new Dictionary<UnitOfMeasureDimension, string>
        {
            [UnitOfMeasureDimension.Count] = "ea",
            [UnitOfMeasureDimension.Mass] = "kg",
            [UnitOfMeasureDimension.Volume] = "l",
            [UnitOfMeasureDimension.Length] = "m"
        };

        var dimensions = defaultTargets.Keys.ToArray();
        var existingDefaults = await _dbContext.CompanyUnitsOfMeasure
            .Where(item => item.CompanyId == company.Id && dimensions.Contains(item.Dimension) && item.IsDefaultForDimension)
            .ToListAsync(cancellationToken);

        var defaultResults = new List<RfidSeedUomDefaultResult>(defaultTargets.Count);
        foreach (var (dimension, code) in defaultTargets)
        {
            var target = companyUnitsByCode[code];
            var defaultsForDimension = existingDefaults
                .Where(item => item.Dimension == dimension)
                .ToList();

            var hasTargetDefault = defaultsForDimension.Any(item => item.UnitOfMeasureId == target.UnitOfMeasureId);
            var changed = false;

            foreach (var defaultUnit in defaultsForDimension)
            {
                if (defaultUnit.UnitOfMeasureId == target.UnitOfMeasureId)
                {
                    continue;
                }

                defaultUnit.SetDefaultForDimension(false);
                changed = true;
            }

            if (!hasTargetDefault || !target.IsDefaultForDimension)
            {
                target.SetDefaultForDimension(true);
                changed = true;
            }

            defaultResults.Add(new RfidSeedUomDefaultResult(dimension, code, changed ? "set" : "already_set"));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RfidSeedUomResult(
            unitResults,
            defaultResults,
            "RFID UOM seed completed.");
    }

    private async Task<Organization> EnsureOrganizationAsync(CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .FirstOrDefaultAsync(
                item => item.DisplayName == CompanyName && item.AccountType == OrganizationType.MultiCompany,
                cancellationToken);

        if (organization is not null)
        {
            return organization;
        }

        organization = Organization.Create(OrganizationType.MultiCompany, CompanyName);
        await _dbContext.Organizations.AddAsync(organization, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return organization;
    }

    private async Task<Company> EnsureCompanyAsync(Organization organization, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(item => item.Name == CompanyName, cancellationToken);

        if (company is not null)
        {
            return company;
        }

        company = Company.Create(organization.Id, 0, CompanyName);
        await _dbContext.Companies.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return company;
    }

    private async Task<TaxEntity> EnsureTaxEntityAsync(Company company, CancellationToken cancellationToken)
    {
        TaxEntity? taxEntity = null;

        if (company.TaxEntityId > 0)
        {
            taxEntity = await _dbContext.TaxEntities
                .FirstOrDefaultAsync(item => item.Id == company.TaxEntityId, cancellationToken);
        }

        taxEntity ??= await _dbContext.TaxEntities
            .FirstOrDefaultAsync(item => item.CompanyPublicId == company.PublicId, cancellationToken);

        if (taxEntity is null)
        {
            taxEntity = TaxEntity.Create(company.PublicId, CompanyTaxId, CompanyName);
            await _dbContext.TaxEntities.AddAsync(taxEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(taxEntity.DisplayName, CompanyName, StringComparison.Ordinal))
        {
            taxEntity.UpdateDisplayName(CompanyName);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (company.TaxEntityId != taxEntity.Id)
        {
            company.UpdateTaxEntityId(taxEntity.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return taxEntity;
    }

    private async Task<Warehouse> EnsureWarehouseAsync(Company company, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses
            .FirstOrDefaultAsync(item => item.CompanyId == company.Id && item.Code == WarehouseCode, cancellationToken);

        if (warehouse is not null)
        {
            warehouse.Update(WarehouseCode, WarehouseName, true);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return warehouse;
        }

        warehouse = Warehouse.Create(company.Id, WarehouseCode, WarehouseName);
        await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return warehouse;
    }

    private async Task<(RfidTag FirstTag, string FirstEpc)> EnsureTagsAsync(Guid companyPublicId, Guid productPublicId, IReadOnlyList<string> epcs, CancellationToken cancellationToken)
    {
        if (epcs.Count == 0)
        {
            throw new InvalidOperationException("RFID seed EPC list is empty.");
        }

        var normalizedEpcs = epcs.Select(item => item.Trim().ToUpperInvariant()).ToArray();
        if (normalizedEpcs.Any(item => item.Length != 24 || !item.All(Uri.IsHexDigit)))
        {
            throw new InvalidOperationException("RFID seed EPC list contains invalid SGTIN-96 hexadecimal values.");
        }

        if (normalizedEpcs.Distinct(StringComparer.Ordinal).Count() != normalizedEpcs.Length)
        {
            throw new InvalidOperationException("RFID seed EPC list contains duplicates.");
        }

        var existingTags = await _dbContext.RfidTags
            .Where(item => item.CompanyPublicId == companyPublicId && normalizedEpcs.Contains(item.Epc))
            .ToDictionaryAsync(item => item.Epc, item => item, cancellationToken);

        RfidTag? firstTag = null;
        string? firstEpc = null;

        foreach (var epc in normalizedEpcs)
        {
            if (existingTags.TryGetValue(epc, out var existingTag))
            {
                existingTag.Update(RfidTagStatus.Active, "Registro de ejemplo para semilla RFID.");
                firstTag ??= existingTag;
                firstEpc ??= epc;
                continue;
            }

            var newTag = RfidTag.Create(companyPublicId, warehousePublicId: null, epc, "Registro de ejemplo para semilla RFID.");
            await _dbContext.RfidTags.AddAsync(newTag, cancellationToken);
            firstTag ??= newTag;
            firstEpc ??= epc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (firstTag!, firstEpc!);
    }

    private async Task<RfidEventInbox> EnsureEventInboxAsync(Guid companyPublicId, Guid warehousePublicId, string sampleEpc, CancellationToken cancellationToken)
    {
        var inbox = await _dbContext.RfidEventInboxes
            .FirstOrDefaultAsync(item => item.CompanyPublicId == companyPublicId && item.EventId == EventId, cancellationToken);

        if (inbox is not null)
        {
            return inbox;
        }

        var payload = $"{{\"event\":\"portal-exit\",\"epcs\":[\"{sampleEpc}\"],\"deviceCode\":\"RFID-GATE-BRA\"}}";
        inbox = RfidEventInbox.Create(
            companyPublicId,
            EventId,
            RfidEventType.PortalExit,
            DeviceCode,
            warehousePublicId,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            0.9800m,
            payload);
        await _dbContext.RfidEventInboxes.AddAsync(inbox, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return inbox;
    }

    private async Task<Operator> EnsureOperatorAsync(Guid companyPublicId, CancellationToken cancellationToken)
    {
        var op = await _dbContext.Operators
            .FirstOrDefaultAsync(item => item.CompanyPublicId == companyPublicId && item.DocumentId == SeedOperatorDocumentId, cancellationToken);

        if (op is not null)
        {
            return op;
        }

        op = Operator.Create(companyPublicId, SeedOperatorDocumentId, SeedOperatorName, SeedOperatorDocumentId, isActive: true);
        await _dbContext.Operators.AddAsync(op, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return op;
    }

    private async Task<IReadOnlyList<Product>> EnsureProductsAsync(long companyId, CancellationToken cancellationToken)
    {
        var unit = await _dbContext.UnitOfMeasures
            .FirstOrDefaultAsync(item => item.CanonicalCode == "unit", cancellationToken)
            ?? await _dbContext.UnitOfMeasures
                .FirstOrDefaultAsync(item => item.CanonicalCode == "un", cancellationToken)
            ?? await _dbContext.UnitOfMeasures
                .OrderBy(item => item.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (unit is null)
        {
            unit = UnitOfMeasure.Create(
                canonicalCode: "rfid-unit",
                displayCode: "RFID-UNIT",
                dimension: UnitOfMeasureDimension.Count,
                isBaseUnit: true,
                factorToBase: 1m,
                precisionScale: 0,
                isActive: true,
                sortOrder: null);
            await _dbContext.UnitOfMeasures.AddAsync(unit, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var skus = SeedProducts.Select(item => item.Sku).ToArray();
        var existing = await _dbContext.Products
            .Where(item => item.CompanyId == companyId && skus.Contains(item.Sku))
            .ToDictionaryAsync(item => item.Sku, item => item, cancellationToken);

        var created = false;
        foreach (var (sku, name, barcode) in SeedProducts)
        {
            if (existing.ContainsKey(sku))
            {
                continue;
            }

            var product = Product.Create(
                companyId,
                sku,
                name,
                barcode,
                unit.Id,
                isStockable: true,
                isSellable: true,
                isPurchasable: true);
            await _dbContext.Products.AddAsync(product, cancellationToken);
            created = true;
        }

        if (created)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await _dbContext.Products
            .Where(item => item.CompanyId == companyId && skus.Contains(item.Sku))
            .OrderBy(item => item.Sku)
            .ToListAsync(cancellationToken);
    }

    private async Task<RfidAccessSession> EnsureAccessSessionAsync(Guid companyPublicId, Guid warehousePublicId, long operatorId, CancellationToken cancellationToken)
    {
        var session = await _dbContext.RfidAccessSessions
            .FirstOrDefaultAsync(item => item.CompanyPublicId == companyPublicId && item.SessionId == SessionId, cancellationToken);

        if (session is not null)
        {
            session.UpdateMetadata("CARD-BRA-001", DeviceCode);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return session;
        }

        session = RfidAccessSession.CreateResolved(
            companyPublicId,
            warehousePublicId,
            DeviceCode,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            (int)RfidAccessSessionStatus.Active,
            operatorId,
            SessionId,
            endedAt: null,
            faceTemplateId: null,
            nfcCardUid: "CARD-BRA-001");
        await _dbContext.RfidAccessSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    private async Task<RfidMovementLink> EnsureMovementLinkAsync(Guid companyPublicId, Guid productPublicId, string epc, CancellationToken cancellationToken)
    {
        var link = await _dbContext.RfidMovementLinks
            .FirstOrDefaultAsync(
                item => item.CompanyPublicId == companyPublicId && item.EventId == EventId && item.Epc == epc,
                cancellationToken);

        if (link is not null)
        {
            return link;
        }

        link = RfidMovementLink.Create(companyPublicId, EventId, SessionId, epc, productPublicId, inventoryMovementPublicId: null);
        await _dbContext.RfidMovementLinks.AddAsync(link, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return link;
    }

    private async Task EnsureFeatureCatalogAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var feature = await _dbContext.FeatureCatalog
            .FirstOrDefaultAsync(
                item => item.PublicId == FeatureCode.Rfid.PublicId || item.Code == FeatureCode.Rfid.Code,
                cancellationToken);

        if (feature is null)
        {
            feature = FeatureCatalog.Create(
                FeatureCode.Rfid,
                FeatureCode.Rfid.Name,
                FeatureCode.Rfid.Description,
                isActive: true,
                now);
            await _dbContext.FeatureCatalog.AddAsync(feature, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        feature.UpdateCatalog(
            FeatureCode.Rfid,
            FeatureCode.Rfid.Name,
            FeatureCode.Rfid.Description,
            isActive: true,
            now);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRfidRoleAndPermissionsAsync(CancellationToken cancellationToken)
    {
        var rfidManagerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(item => item.Name == RoleNames.RfidManager, cancellationToken);

        if (rfidManagerRole is null)
        {
            rfidManagerRole = Role.Create(
                RoleNames.RfidManager,
                requiredFeatureKey: FeatureKeys.Rfid,
                isSystem: true,
                scopeType: RoleAssignmentScopeType.Company);
            await _dbContext.Roles.AddAsync(rfidManagerRole, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            rfidManagerRole.Update(RoleNames.RfidManager, rfidManagerRole.Description, isActive: true, scopeType: RoleAssignmentScopeType.Company);
            rfidManagerRole.UpdateMetadata(FeatureKeys.Rfid, isSystem: true);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var rfidPermissionCodes = new[]
        {
            PermissionKeys.Rfid.TagsManage,
            PermissionKeys.Rfid.AccessSessionsManage,
            PermissionKeys.Rfid.EventsManage,
            PermissionKeys.Rfid.MovementLinksManage
        };

        var rfidPermissions = await _dbContext.Permissions
            .Where(item => rfidPermissionCodes.Contains(item.Code))
            .ToListAsync(cancellationToken);

        foreach (var code in rfidPermissionCodes)
        {
            var existing = rfidPermissions.FirstOrDefault(item => item.Code == code);
            if (existing is null)
            {
                existing = Permission.Create(code, BuildPermissionName(code), null, RoleAssignmentScopeType.Company);
                await _dbContext.Permissions.AddAsync(existing, cancellationToken);
                rfidPermissions.Add(existing);
                continue;
            }

            existing.Update(code, BuildPermissionName(code), existing.Description, RoleAssignmentScopeType.Company);
            existing.Enable();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var rfidPermissionIds = rfidPermissions.Select(item => item.Id).ToArray();
        var existingRfidRolePermissionIds = await _dbContext.RolePermissions
            .Where(item => item.RoleId == rfidManagerRole.Id && rfidPermissionIds.Contains(item.PermissionId))
            .Select(item => item.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in rfidPermissions)
        {
            if (existingRfidRolePermissionIds.Contains(permission.Id))
            {
                continue;
            }

            await _dbContext.RolePermissions.AddAsync(RolePermission.Create(rfidManagerRole.Id, permission.Id), cancellationToken);
        }

        var companyOwnerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(item => item.Name == RoleNames.CompanyOwner, cancellationToken);

        if (companyOwnerRole is null)
        {
            companyOwnerRole = Role.Create(
                RoleNames.CompanyOwner,
                requiredFeatureKey: null,
                isSystem: true,
                scopeType: RoleAssignmentScopeType.Company);
            await _dbContext.Roles.AddAsync(companyOwnerRole, cancellationToken);
        }
        else
        {
            companyOwnerRole.Update(
                RoleNames.CompanyOwner,
                companyOwnerRole.Description,
                isActive: true,
                scopeType: RoleAssignmentScopeType.Company);
            companyOwnerRole.UpdateMetadata(requiredFeatureKey: null, isSystem: true);
        }

        var companySuppliersPermission = await _dbContext.Permissions
            .FirstOrDefaultAsync(item => item.Code == PermissionKeys.Company.SuppliersManage, cancellationToken);

        if (companySuppliersPermission is null)
        {
            companySuppliersPermission = Permission.Create(
                PermissionKeys.Company.SuppliersManage,
                BuildPermissionName(PermissionKeys.Company.SuppliersManage),
                description: null,
                scopeType: RoleAssignmentScopeType.Company);
            await _dbContext.Permissions.AddAsync(companySuppliersPermission, cancellationToken);
        }
        else
        {
            companySuppliersPermission.Update(
                PermissionKeys.Company.SuppliersManage,
                BuildPermissionName(PermissionKeys.Company.SuppliersManage),
                companySuppliersPermission.Description,
                RoleAssignmentScopeType.Company);
            companySuppliersPermission.Enable();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var hasCompanyOwnerSuppliersPermission = await _dbContext.RolePermissions
            .AnyAsync(
                item => item.RoleId == companyOwnerRole.Id && item.PermissionId == companySuppliersPermission.Id,
                cancellationToken);

        if (!hasCompanyOwnerSuppliersPermission)
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(companyOwnerRole.Id, companySuppliersPermission.Id),
                cancellationToken);
        }

        var platformSuperAdminRole = await _dbContext.Roles
            .FirstOrDefaultAsync(item => item.Name == RoleNames.PlatformSuperAdmin, cancellationToken);

        if (platformSuperAdminRole is null)
        {
            platformSuperAdminRole = Role.Create(
                RoleNames.PlatformSuperAdmin,
                requiredFeatureKey: null,
                isSystem: true,
                scopeType: RoleAssignmentScopeType.Platform);
            await _dbContext.Roles.AddAsync(platformSuperAdminRole, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            platformSuperAdminRole.Update(
                RoleNames.PlatformSuperAdmin,
                platformSuperAdminRole.Description,
                isActive: true,
                scopeType: RoleAssignmentScopeType.Platform);
            platformSuperAdminRole.UpdateMetadata(requiredFeatureKey: null, isSystem: true);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var platformWsuPermission = await _dbContext.Permissions
            .FirstOrDefaultAsync(item => item.Code == PermissionKeys.Platform.WsuManage, cancellationToken);

        var platformWsuEventsIngestPermission = await _dbContext.Permissions
            .FirstOrDefaultAsync(item => item.Code == PermissionKeys.Platform.WsuEventsIngest, cancellationToken);

        var platformWsuEventsConsumptionPermission = await _dbContext.Permissions
            .FirstOrDefaultAsync(item => item.Code == PermissionKeys.Platform.WsuEventsConsumption, cancellationToken);

        if (platformWsuPermission is null)
        {
            platformWsuPermission = Permission.Create(
                PermissionKeys.Platform.WsuManage,
                BuildPermissionName(PermissionKeys.Platform.WsuManage),
                description: null,
                scopeType: RoleAssignmentScopeType.Platform);
            await _dbContext.Permissions.AddAsync(platformWsuPermission, cancellationToken);
        }
        else
        {
            platformWsuPermission.Update(
                PermissionKeys.Platform.WsuManage,
                BuildPermissionName(PermissionKeys.Platform.WsuManage),
                platformWsuPermission.Description,
                RoleAssignmentScopeType.Platform);
            platformWsuPermission.Enable();
        }

        if (platformWsuEventsIngestPermission is null)
        {
            platformWsuEventsIngestPermission = Permission.Create(
                PermissionKeys.Platform.WsuEventsIngest,
                BuildPermissionName(PermissionKeys.Platform.WsuEventsIngest),
                description: null,
                scopeType: RoleAssignmentScopeType.Platform);
            await _dbContext.Permissions.AddAsync(platformWsuEventsIngestPermission, cancellationToken);
        }
        else
        {
            platformWsuEventsIngestPermission.Update(
                PermissionKeys.Platform.WsuEventsIngest,
                BuildPermissionName(PermissionKeys.Platform.WsuEventsIngest),
                platformWsuEventsIngestPermission.Description,
                RoleAssignmentScopeType.Platform);
            platformWsuEventsIngestPermission.Enable();
        }

        if (platformWsuEventsConsumptionPermission is null)
        {
            platformWsuEventsConsumptionPermission = Permission.Create(
                PermissionKeys.Platform.WsuEventsConsumption,
                BuildPermissionName(PermissionKeys.Platform.WsuEventsConsumption),
                description: null,
                scopeType: RoleAssignmentScopeType.Platform);
            await _dbContext.Permissions.AddAsync(platformWsuEventsConsumptionPermission, cancellationToken);
        }
        else
        {
            platformWsuEventsConsumptionPermission.Update(
                PermissionKeys.Platform.WsuEventsConsumption,
                BuildPermissionName(PermissionKeys.Platform.WsuEventsConsumption),
                platformWsuEventsConsumptionPermission.Description,
                RoleAssignmentScopeType.Platform);
            platformWsuEventsConsumptionPermission.Enable();
        }

        var systemEventProcessorRole = await _dbContext.Roles
            .FirstOrDefaultAsync(item => item.Name == RoleNames.SystemEventProcessor, cancellationToken);

        if (systemEventProcessorRole is null)
        {
            systemEventProcessorRole = Role.Create(
                RoleNames.SystemEventProcessor,
                requiredFeatureKey: null,
                isSystem: true,
                scopeType: RoleAssignmentScopeType.Platform);
            await _dbContext.Roles.AddAsync(systemEventProcessorRole, cancellationToken);
        }
        else
        {
            systemEventProcessorRole.Update(
                RoleNames.SystemEventProcessor,
                systemEventProcessorRole.Description,
                isActive: true,
                scopeType: RoleAssignmentScopeType.Platform);
            systemEventProcessorRole.UpdateMetadata(requiredFeatureKey: null, isSystem: true);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var hasPlatformRolePermission = await _dbContext.RolePermissions
            .AnyAsync(
                item => item.RoleId == platformSuperAdminRole.Id && item.PermissionId == platformWsuPermission.Id,
                cancellationToken);

        if (!hasPlatformRolePermission)
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(platformSuperAdminRole.Id, platformWsuPermission.Id),
                cancellationToken);
        }

        var hasPlatformSuperAdminConsumptionRolePermission = await _dbContext.RolePermissions
            .AnyAsync(
                item => item.RoleId == platformSuperAdminRole.Id && item.PermissionId == platformWsuEventsConsumptionPermission.Id,
                cancellationToken);

        if (!hasPlatformSuperAdminConsumptionRolePermission)
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(platformSuperAdminRole.Id, platformWsuEventsConsumptionPermission.Id),
                cancellationToken);
        }

        var hasSystemProcessorRolePermission = await _dbContext.RolePermissions
            .AnyAsync(
                item => item.RoleId == systemEventProcessorRole.Id && item.PermissionId == platformWsuEventsIngestPermission.Id,
                cancellationToken);

        if (!hasSystemProcessorRolePermission)
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(systemEventProcessorRole.Id, platformWsuEventsIngestPermission.Id),
                cancellationToken);
        }

        var hasSystemProcessorConsumptionRolePermission = await _dbContext.RolePermissions
            .AnyAsync(
                item => item.RoleId == systemEventProcessorRole.Id && item.PermissionId == platformWsuEventsConsumptionPermission.Id,
                cancellationToken);

        if (!hasSystemProcessorConsumptionRolePermission)
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(systemEventProcessorRole.Id, platformWsuEventsConsumptionPermission.Id),
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildPermissionName(string code)
    {
        return code.Replace(".", " ", StringComparison.Ordinal);
    }
}

public sealed record RfidSeedResult(
    long OrganizationId,
    long CompanyId,
    long TaxEntityId,
    long WarehouseId,
    int ProductsCount,
    long TagId,
    long EventInboxId,
    long AccessSessionId,
    long MovementLinkId,
    string Message);

public sealed record RfidSeedUomResult(
    IReadOnlyList<RfidSeedUomItemResult> Units,
    IReadOnlyList<RfidSeedUomDefaultResult> Defaults,
    string Message);

public sealed record RfidSeedUomItemResult(
    string CanonicalCode,
    long UnitOfMeasureId,
    string Action);

public sealed record RfidSeedUomDefaultResult(
    UnitOfMeasureDimension Dimension,
    string CanonicalCode,
    string Action);
