using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;
using GeneratedProjectName.Contract;

namespace GeneratedProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CalculatorController : ControllerBase
    {
        public CalculatorController(
            ILogger<CalculatorController> logger,
            IClusterClient orleansClient)
        {
            OrleansClient = orleansClient;
            Logger = logger;
        }

        public IClusterClient OrleansClient { get; }
        public ILogger Logger { get; }

        /// <summary>
        /// Adds two numbers provided
        /// </summary>
        /// <param name="l">An integer to add</param>
        /// <param name="r">An integer to add</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Adder/4+5
        ///
        /// </remarks>
        /// <returns>The sum of the two numbers provided.</returns>
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{l}+{r}", Name = "Add")]
        public virtual async Task<IActionResult> Add(int l, int r)
        {
            var adderGrain = OrleansClient.GetGrain<ICalculatorGrain>(Guid.NewGuid());
            var result = await adderGrain.Add(l, r);
            return Ok(result);
        }
    }
}
