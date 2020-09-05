using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly ILogger<AnalysisController> _logger;

        private readonly AnalysisService _analysisService;

        public AnalysisController(ILogger<AnalysisController> logger,
            AnalysisService analysisService)
        {
            _logger = logger;
            _analysisService = analysisService;
        }

        [HttpPost]
        public async Task<Protocols.Response.Analysis> Execute([FromBody] Protocols.Request.Analysis analysis)
        {
            return await _analysisService.Execute(analysis);
        }
    }
}
