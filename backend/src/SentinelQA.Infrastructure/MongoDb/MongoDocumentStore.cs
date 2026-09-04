using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using SentinelQA.Application.Abstractions.Infrastructure;

namespace SentinelQA.Infrastructure.MongoDb;

public sealed class MongoDocumentStore : IDocumentStore
{
    private readonly IMongoDatabase _database;

    public MongoDocumentStore(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017";
        var databaseName = configuration["Mongo:Database"] ?? "sentinelqa";

        _database = new MongoClient(connectionString).GetDatabase(databaseName);
    }

    public async Task SaveAsync<T>(string collectionName, T document, CancellationToken cancellationToken = default) =>
        await _database.GetCollection<T>(collectionName).InsertOneAsync(document, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync<T>(string collectionName, string fieldName, Guid value, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(fieldName, value);
        return await _database.GetCollection<T>(collectionName).Find(filter).ToListAsync(cancellationToken);
    }
}