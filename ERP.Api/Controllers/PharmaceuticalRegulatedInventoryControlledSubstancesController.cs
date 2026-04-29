using System.Text;
using ERP.Api.Authorization;
using ERP.Api.Contracts.PharmaceuticalRegulatedInventory;
using ERP.Api.Filters;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Commands;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Handlers;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[RequireFeature(PharmacyFeatureKeys.ComplianceIsp)]
[Route("api/pharmaceutical-regulated-inventory/controlled-substances")]
public sealed class PharmaceuticalRegulatedInventoryControlledSubstancesController : ControllerBase
{
    private readonly IOfficialControlledBookEntryRepository _entryRepository;
    private readonly ControlledSubstanceLedgerCommandHandler _commandHandler;
    private readonly IControlledSubstanceReportExportRepository _exportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public PharmaceuticalRegulatedInventoryControlledSubstancesController(
        IOfficialControlledBookEntryRepository entryRepository,
        ControlledSubstanceLedgerCommandHandler commandHandler,
        IControlledSubstanceReportExportRepository exportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _entryRepository = entryRepository;
        _commandHandler = commandHandler;
        _exportRepository = exportRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("entries")]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookEntriesCreate)]
    public async Task<IActionResult> RecordEntry(ControlledSubstanceEntryRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _commandHandler.HandleAsync(
            new RecordControlledSubstanceEntryCommand(
                request.CompanyId,
                request.Date,
                request.ProductId,
                request.BatchNumber,
                request.Quantity,
                request.ReferenceDocument,
                request.IspFolio,
                request.IspReason,
                request.IspOriginDestination,
                request.IspSupplierOrPatient,
                request.IspPrescriber,
                request.IspTaxReference,
                currentUser.Email),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok", entryId = result.Value?.Id });
    }

    [HttpPost("exits")]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookExitsCreate)]
    public async Task<IActionResult> RecordExit(ControlledSubstanceExitRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _commandHandler.HandleAsync(
            new RecordControlledSubstanceExitCommand(
                request.CompanyId,
                request.Date,
                request.ProductId,
                request.BatchNumber,
                request.Quantity,
                request.ReferenceDocument,
                request.IspFolio,
                request.IspReason,
                request.IspOriginDestination,
                request.IspSupplierOrPatient,
                request.IspPrescriber,
                request.IspTaxReference,
                currentUser.Email),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok", entryId = result.Value?.Id });
    }

    [HttpPost("adjustments")]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookAdjustmentsCreate)]
    public async Task<IActionResult> RecordAdjustment(ControlledSubstanceAdjustmentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var currentUser))
        {
            return Unauthorized();
        }

        if (request.CompanyId != currentUser.CompanyId)
        {
            return Forbid();
        }

        var result = await _commandHandler.HandleAsync(
            new RecordControlledSubstanceAdjustmentCommand(
                request.CompanyId,
                request.Date,
                request.ProductId,
                request.BatchNumber,
                request.Quantity,
                request.IsIncrease,
                request.ReferenceDocument,
                request.IspFolio,
                request.IspReason,
                request.IspOriginDestination,
                request.IspSupplierOrPatient,
                request.IspPrescriber,
                request.IspTaxReference,
                currentUser.Email),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok", entryId = result.Value?.Id });
    }

    [HttpGet("ledger")]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookRead)]
    public async Task<IActionResult> GetLedger([FromQuery] ControlledSubstanceLedgerQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entries = await _entryRepository.ListAsync(
            companyId,
            query.From,
            query.To,
            query.ProductId,
            query.MovementType,
            query.BatchNumber,
            query.IspFolio,
            cancellationToken);

        var response = entries.Select(entry => new
        {
            entry.Id,
            entry.Date,
            entry.ProductId,
            entry.BatchNumber,
            entry.Quantity,
            entry.MovementType,
            entry.ReferenceDocument,
            entry.IspFolio,
            entry.IspReason,
            entry.IspOriginDestination,
            entry.IspSupplierOrPatient,
            entry.IspPrescriber,
            entry.IspTaxReference,
            entry.CreatedBy
        });

        return Ok(response);
    }

    [HttpGet("ledger/export")]
    [RequireCompanyPermission(PermissionKeys.Pharmaceutical.ControlledBookExport)]
    public async Task<IActionResult> ExportLedger([FromQuery] ControlledSubstanceLedgerExportQuery query, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var entries = await _entryRepository.ListAsync(
            companyId,
            query.From,
            query.To,
            query.ProductId,
            query.MovementType,
            query.BatchNumber,
            query.IspFolio,
            cancellationToken);

        if (TryGetCurrentUser(out var currentUser))
        {
            var export = ControlledSubstanceReportExport.Create(
                companyId,
                DateTime.UtcNow,
                query.Format.ToString(),
                query.From,
                query.To,
                currentUser.Email);
            await _exportRepository.AddAsync(export, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (query.Format == ControlledSubstanceExportFormat.Csv)
        {
            var csv = ToCsv(entries);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "controlled-substances-ledger.csv");
        }

        var response = entries.Select(entry => new
        {
            entry.Id,
            entry.Date,
            entry.ProductId,
            entry.BatchNumber,
            entry.Quantity,
            entry.MovementType,
            entry.ReferenceDocument,
            entry.IspFolio,
            entry.IspReason,
            entry.IspOriginDestination,
            entry.IspSupplierOrPatient,
            entry.IspPrescriber,
            entry.IspTaxReference,
            entry.CreatedBy
        });

        return Ok(response);
    }

    private static string ToCsv(IEnumerable<OfficialControlledBookEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,ProductId,BatchNumber,Quantity,MovementType,ReferenceDocument,IspFolio,IspReason,IspOriginDestination,IspSupplierOrPatient,IspPrescriber,IspTaxReference,CreatedBy");
        foreach (var entry in entries)
        {
            sb.AppendLine(string.Join(
                ",",
                CsvValue(entry.Date.ToString("O")),
                CsvValue(entry.ProductId.ToString()),
                CsvValue(entry.BatchNumber),
                CsvValue(entry.Quantity.ToString("0.##")),
                CsvValue(entry.MovementType.ToString()),
                CsvValue(entry.ReferenceDocument),
                CsvValue(entry.IspFolio),
                CsvValue(entry.IspReason),
                CsvValue(entry.IspOriginDestination),
                CsvValue(entry.IspSupplierOrPatient),
                CsvValue(entry.IspPrescriber),
                CsvValue(entry.IspTaxReference),
                CsvValue(entry.CreatedBy)));
        }

        return sb.ToString();
    }

    private static string CsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }

    private bool TryGetCurrentUser(out CurrentUser currentUser)
    {
        var user = _currentUserProvider.GetCurrentUser();
        if (user is null)
        {
            currentUser = default!;
            return false;
        }

        currentUser = user;
        return true;
    }
}
