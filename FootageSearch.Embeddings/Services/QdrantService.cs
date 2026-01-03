using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootageSearch.Embeddings.Interfaces;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace FootageSearch.Embeddings.Services
{
    public class QdrantService : IVectorDbService
    {
        private readonly QdrantClient _client;
        private const string CollectionName = "footage_search";
        private const int VectorSize = 384; // Example size (e.g., for all-MiniLM-L6-v2)

        public QdrantService()
        {
            _client = new QdrantClient("localhost", 6334);
        }

        public async Task InitializeAsync()
        {
            var collections = await _client.ListCollectionsAsync();
            if (!collections.Contains(CollectionName))
            {
                await _client.CreateCollectionAsync(CollectionName, new VectorParams { Size = VectorSize, Distance = Distance.Cosine });
            }
        }

        public async Task UpsertAsync(Guid id, float[] vector, Dictionary<string, string> payload)
        {
            var point = new PointStruct
            {
                Id = id,
                Vectors = vector
            };
            
            foreach(var kvp in payload)
            {
                point.Payload.Add(kvp.Key, kvp.Value);
            }

            await _client.UpsertAsync(CollectionName, new[] { point });
        }

        public async Task<List<(Guid Id, double Score)>> SearchAsync(float[] vector, int limit = 10)
        {
            var results = await _client.SearchAsync(CollectionName, vector, limit: (ulong)limit);
            return results.Select(r => (Guid.Parse(r.Id.Uuid), (double)r.Score)).ToList();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _client.DeleteAsync(CollectionName, new[] { id });
        }

        public async Task DeleteCollectionAsync()
        {
            var collections = await _client.ListCollectionsAsync();
            if (collections.Contains(CollectionName))
            {
                await _client.DeleteCollectionAsync(CollectionName);
            }
        }
    }
}
