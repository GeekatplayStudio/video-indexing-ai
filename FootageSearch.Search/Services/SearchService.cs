using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootageSearch.Data;
using FootageSearch.Data.Models;
using FootageSearch.Embeddings.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootageSearch.Search.Services
{
    public class SearchService
    {
        private readonly VideoDbContext _dbContext;
        private readonly IVectorDbService _vectorDbService;

        public SearchService(VideoDbContext dbContext, IVectorDbService vectorDbService)
        {
            _dbContext = dbContext;
            _vectorDbService = vectorDbService;
        }

        public async Task<List<VideoFile>> SearchAsync(string query)
        {
            // 1. Simple Metadata Search (Filename)
            var metadataResults = await _dbContext.VideoFiles
                .Where(v => EF.Functions.Like(v.FileName, $"%{query}%"))
                .ToListAsync();

            // 2. Vector Search (Placeholder - would need to embed query)
            // var vector = Embed(query);
            // var vectorResults = await _vectorDbService.SearchAsync(vector);
            // var ids = vectorResults.Select(r => r.Id).ToList();
            // var dbResults = await _dbContext.VideoFiles.Where(v => ids.Contains(v.VectorId)).ToListAsync();

            return metadataResults;
        }
        
        public async Task<List<VideoFile>> GetAllFilesAsync()
        {
            return await _dbContext.VideoFiles.ToListAsync();
        }
    }
}
