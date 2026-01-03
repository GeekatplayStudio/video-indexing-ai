using Microsoft.AspNetCore.Mvc;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;
using FootageSearch.Data;
using FootageSearch.Data.Services;

namespace FootageSearch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly VideoDbContext _dbContext;

        public StatusController(VideoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<JobStatus>> Get()
        {
            // We can reuse the logic from DbJobStatusService or just query directly
            var service = new DbJobStatusService(_dbContext, "Indexer");
            var status = await service.GetStatusAsync();
            return Ok(status);
        }
    }
}
