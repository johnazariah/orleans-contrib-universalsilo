namespace Orleans.Contrib.UniversalSilo

open Microsoft.Extensions.Configuration
open Orleans
open Orleans.Hosting
open Orleans.TestingHost
open System

type TestSiloConfigurator() = class
    let mutable _configuration : IConfiguration = null

    member val Configuration = _configuration with get

    interface ISiloConfigurator with
        member __.Configure siloBuilder =
            _configuration <- siloBuilder.GetConfiguration()

            siloBuilder
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .ConfigureApplicationParts(fun parts ->
                    parts.AddFromApplicationBaseDirectory()
                    |> (fun apm -> apm.WithCodeGeneration())
                    |> ignore)
            |> ignore
end

type ClusterFixture() = class
    let mutable _cluster : TestCluster = null
    do
        _cluster <-
            TestClusterBuilder()
                .ConfigureHostConfiguration(fun builder -> builder.AddEnvironmentVariables () |> ignore)
                .AddSiloBuilderConfigurator<TestSiloConfigurator>()
                .Build()

        _cluster.Deploy()

    member val Cluster = _cluster with get

    interface IDisposable with
        member __.Dispose () =
            _cluster.StopAllSilos ()
end
