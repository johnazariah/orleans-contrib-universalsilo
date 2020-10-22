namespace GeneratedProjectName.Controllers
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Orleans
open System
open System.Threading.Tasks
open GeneratedProjectName.Contract

[<ApiController>]
[<Route("api/[controller]")>]
[<Produces("application/json")>]
type public CalculatorController (logger, orleansClient) = class
    inherit ControllerBase()
    member val OrleansClient : IClusterClient = orleansClient
    member val Logger : ILogger = logger

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
    [<ProducesResponseType(typeof<int>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    [<HttpGet("{l}+{r}", Name = "Add")>]
    abstract Add : int -> int -> Task<int>
    default this.Add l r =
        let adderGrain = this.OrleansClient.GetGrain<ICalculatorGrain> <| Guid.NewGuid()
        adderGrain.Add l r
end