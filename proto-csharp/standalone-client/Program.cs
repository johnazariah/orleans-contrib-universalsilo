using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Contrib.UniversalSilo.ClusterClient;
using System;
using System.Threading.Tasks;
using GeneratedProjectName.Contract;
using static Orleans.Contrib.UniversalSilo.Configuration;
using Orleans.Contrib.UniversalSilo;

namespace GeneratedProjectName.StandaloneClient
{
    /// <summary>
    /// This is the service that will host your client application.
    /// </summary>
    class ClientService : HostedServiceBase
    {

        // These parameters are dependency injected, and passed to the base class to expose as properties.
        // <param name="applicationLifetime">This is exposed as the `ApplicationLifetime` property</param>
        // <param name="clusterClient">This is exposed as the `ClusterClient` property</param>
        public ClientService(IHostApplicationLifetime applicationLifetime, IClusterClient clusterClient) :
            base(applicationLifetime, clusterClient)
        { }

        // Put the code that needs to be executed against the silo in this function.
        //
        // As the example shows, you have access to the `ClusterClient` from where grains can be requested.
        //
        // This example shows a single operation on a grain, after which the client application terminates.
        // For long-running clients and other patterns, refer to
        //   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
        //
        // The `ClientConfiguration` host tries to reach a silo as specified by the configuration, and
        // retries 9 times with a 10 second wait between each retry until it either reaches the silo, or finally times out.
        //
        // This function will *only* be executed when the silo has been found and the client has connected to it.
        public override async Task ExecuteAsync()
        {
            var (l, r) = (5, 6);
            var adder = ClusterClient.GetGrain<ICalculatorGrain>(Guid.NewGuid());
            var result = await adder.Add(l, r);

            Console.WriteLine($"{l} + {r} = {result}");
        }
    }

    class ClientConfiguration : ClientConfiguration<ClientService>
    {
        public override SiloConfiguration SiloConfiguration =>
            base.SiloConfiguration
            .With(_c => _c.ServiceId = "GeneratedProjectName");
    }

    /// <summary>
    ///
    /// This is the entry point to the client.
    ///
    /// No changes should normally be needed here to get a running client finding and talking to a silo
    ///
    /// Provide the configuration of the silo to connect by any combination of (in order of override)
    ///    * The default configuration
    ///    * Overriding in the <see cref="ClientConfiguration"/> class
    ///    * Providing a section in the "appSettings.json"/> file. (If at all possible, do not use this option.)
    ///    * Setting user secrets for managing secrets and connection strings in development
    ///    * Setting environment variables
    ///
    /// </summary>
    class Program
    {
        static async Task Main(string[] args) =>
            await new ClientConfiguration()
                .GetHostBuilder(args)
                .RunConsoleAsync();
    }
}
