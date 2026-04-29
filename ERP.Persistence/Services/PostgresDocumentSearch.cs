using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NpgsqlTypes;

namespace ERP.Persistence.Services;

public sealed class PostgresDocumentSearch : IDocumentSearch
{
    private readonly ErpDbContext _dbContext;

    public PostgresDocumentSearch(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task IndexAsync(DocumentIndexRequest request, CancellationToken cancellationToken = default)
    {
        var documentType = request.DocumentType.Trim();
        var documentId = request.DocumentId.Trim();
        var entry = await _dbContext.DocumentSearchEntries
            .FirstOrDefaultAsync(
                candidate => candidate.CompanyId == request.CompanyId
                    && candidate.DocumentType == documentType
                    && candidate.DocumentId == documentId,
                cancellationToken);

        if (entry is null)
        {
            entry = DocumentSearchEntry.Create(
                request.CompanyId,
                documentType,
                documentId,
                request.Title,
                request.Content,
                request.Metadata);
            await _dbContext.DocumentSearchEntries.AddAsync(entry, cancellationToken);
        }
        else
        {
            entry.Update(request.Title, request.Content, request.Metadata);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(long companyId, string documentType, string documentId, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.DocumentSearchEntries
            .FirstOrDefaultAsync(
                candidate => candidate.CompanyId == companyId
                    && candidate.DocumentType == documentType
                    && candidate.DocumentId == documentId,
                cancellationToken);

        if (entry is null)
        {
            return;
        }

        _dbContext.DocumentSearchEntries.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default)
    {
        var entries = _dbContext.DocumentSearchEntries.AsNoTracking()
            .Where(entry => entry.CompanyId == query.CompanyId);

        if (query.DocumentTypes is { Count: > 0 })
        {
            var documentTypes = query.DocumentTypes
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Select(type => type.Trim())
                .ToArray();

            entries = entries.Where(entry => documentTypes.Contains(entry.DocumentType));
        }

        if (query.Metadata is { Count: > 0 })
        {
            foreach (var filter in query.Metadata)
            {
                var criteria = new Dictionary<string, string> { [filter.Key] = filter.Value };
                entries = entries.Where(entry => EF.Functions.JsonContains(entry.Metadata, criteria));
            }
        }

        var limit = query.Limit <= 0 ? 50 : query.Limit;

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var tsQuery = EF.Functions.WebSearchToTsQuery("simple", query.Text);

            var ranked = await entries
                .Where(entry => EF.Property<NpgsqlTsVector>(entry, "SearchVector").Matches(tsQuery))
                .Select(entry => new
                {
                    entry.DocumentType,
                    entry.DocumentId,
                    entry.Metadata,
                    Score = 0f
                })
                .OrderByDescending(entry => entry.Score)
                .ThenBy(entry => entry.DocumentType)
                .ThenBy(entry => entry.DocumentId)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return ranked
                .Select(entry => new DocumentSearchResult(
                    entry.DocumentType,
                    entry.DocumentId,
                    entry.Score,
                    entry.Metadata))
                .ToList();
        }

        var results = await entries
            .OrderBy(entry => entry.DocumentType)
            .ThenBy(entry => entry.DocumentId)
            .Take(limit)
            .Select(entry => new DocumentSearchResult(
                entry.DocumentType,
                entry.DocumentId,
                0f,
                entry.Metadata))
            .ToListAsync(cancellationToken);

        return results;
    }
}
