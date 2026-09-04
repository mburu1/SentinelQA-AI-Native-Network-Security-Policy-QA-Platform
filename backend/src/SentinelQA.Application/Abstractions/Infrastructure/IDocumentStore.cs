namespace SentinelQA.Application.Abstractions.Infrastructure;

/// <summary>MongoDB-backed store for variable-shape diagnostic documents.</summary>
public interface IDocumentStore
{
    Task SaveAsync<T>(string collectionName, T document, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync<T>(string collectionName, string fieldName, Guid value, CancellationToken cancellationToken = default);
}