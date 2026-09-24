using Microsoft.Azure.Cosmos;
using SupportWebApp.Models;

namespace SupportWebApp.Services;

public class CosmosSupportService
{
    private readonly Container _container;

    public CosmosSupportService(string connectionString, string databaseName, string containerName)
    {
        var client = new CosmosClient(connectionString, new CosmosClientOptions
        {
            // Id -> "id", Category -> "category" osv.
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        });
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task AddAsync(SupportMessage message)
    {
        // Partition key = /category
        await _container.CreateItemAsync(message, new PartitionKey(message.Category));
    }

    public async Task<List<SupportMessage>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");
        var iterator = _container.GetItemQueryIterator<SupportMessage>(query);

        var results = new List<SupportMessage>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            results.AddRange(page);
        }
        return results;
    }
}