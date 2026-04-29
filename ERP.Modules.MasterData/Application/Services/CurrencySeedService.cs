using System.Reflection;
using System.Text.Json;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;
using Microsoft.Extensions.Logging;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class CurrencySeedService : ICurrencySeedService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CurrencySeedService> _logger;
    private readonly Assembly _assembly;

    public CurrencySeedService(
        ICurrencyRepository currencyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CurrencySeedService> logger)
    {
        _currencyRepository = currencyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;

        // Carga el assembly donde está embebido SeedData (ERP.Persistence)
        _assembly = Assembly.Load("ERP.Persistence");
    }

    public async Task<Result> SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting currencies seeding");

        try
        {
            var existingCount = await _currencyRepository.CountAsync(cancellationToken);
            if (existingCount > 0)
            {
                _logger.LogInformation("Currencies already exist ({Count}). Skipping currency seeding.", existingCount);
                return Result.Ok();
            }

            var currencyData = await LoadAsync(cancellationToken);
            if (currencyData.Count == 0)
            {
                _logger.LogWarning("No currency data found to seed");
                return Result.Ok();
            }

            var entities = currencyData
                .Select(c => Currency.Create(
                    c.Code, c.NumericCode, c.Name, c.Symbol, c.MinorUnits, c.Order, c.IsActive))
                .ToArray();

            await _currencyRepository.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} currencies", entities.Length);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding currencies");
            return Result.Fail($"Error seeding currencies: {ex.Message}");
        }
    }

    private async Task<IReadOnlyList<CurrencyDto>> LoadAsync(CancellationToken cancellationToken = default)
    {
        const string resourceName = "ERP.Persistence.SeedData.currency.currencies.prioritized.json";

        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            _logger.LogWarning("Currency seed resource not found: {ResourceName}", resourceName);

            // Diagnóstico (si no calza el nombre)
            // _logger.LogWarning("Available: {Resources}", string.Join(", ", _assembly.GetManifestResourceNames()));

            return Array.Empty<CurrencyDto>();
        }

        var items = await JsonSerializer.DeserializeAsync<List<CurrencyDto>>(stream, SerializerOptions, cancellationToken);
        return items ?? new List<CurrencyDto>();
    }

    private record CurrencyDto(
        string Code,
        int NumericCode,
        string Name,
        string? Symbol,
        int MinorUnits,
        int Order,
        bool IsActive
    );
}
