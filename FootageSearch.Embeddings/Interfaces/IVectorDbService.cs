using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace FootageSearch.Embeddings.Interfaces
{
    public interface IVectorDbService
    {
        Task InitializeAsync();
        Task UpsertAsync(Guid id, float[] vector, Dictionary<string, string> payload);
        Task<List<(Guid Id, double Score)>> SearchAsync(float[] vector, int limit = 10);
        Task DeleteAsync(Guid id);
        Task DeleteCollectionAsync();
    }
}
