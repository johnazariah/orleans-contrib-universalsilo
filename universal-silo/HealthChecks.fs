/// This grain is _directly_ inspired by the work of @RehanSaeedUK in https://github.com/Dotnet-Boxed/Templates
namespace Orleans.Contrib.UniversalSilo


[<AutoOpen>]
module HealthChecks =
    open System
    open System.Collections.Generic
    open Orleans.Runtime
    open System.Threading
    open Orleans
    open Orleans.Concurrency
    open Microsoft.Extensions.Diagnostics.HealthChecks
    open Microsoft.Extensions.Logging

    open System.Threading.Tasks

    [<AutoOpen>]
    module Contract =
        type IHealthCheckGrain = interface
            inherit IGrainWithGuidKey
            abstract CheckAsync : unit -> Task
        end

        type ILocalHealthCheckGrain = interface
            inherit IHealthCheckGrain
        end

        type IStorageHealthCheckGrain = interface
            inherit IHealthCheckGrain
        end

    [<AutoOpen>]
    module Implementation =

        [<StatelessWorker>]
        type LocalHealthCheckGrain() = class
            inherit Orleans.Grain()

            interface ILocalHealthCheckGrain with
                member __.CheckAsync() =
                    Task.CompletedTask
        end

        type StorageHealthCheckState() = class
            member val Value : Guid = Guid.Empty with get, set
        end

        type StorageHealthCheckGrain() = class
            inherit Orleans.Grain<StorageHealthCheckState>()

            interface IStorageHealthCheckGrain with
                member this.CheckAsync() =
                    try
                        let input = StorageHealthCheckState (Value = Guid.NewGuid())

                        this.State <- input
                        this.WriteStateAsync()  |> Async.AwaitTask |> Async.StartAsTask |> ignore

                        this.State.Value <- Guid.Empty
                        this.ReadStateAsync()   |> Async.AwaitTask |> Async.StartAsTask |> ignore

                        if (this.State.Value <> input.Value) then
                            failwith "Storage Persistence Failure: Could not read back stored value faithfully"
                        else
                            Task.CompletedTask
                    finally
                        this.ClearStateAsync() |> Async.AwaitTask |> Async.StartAsTask |> ignore
        end

    type GrainHealthCheck<'hcg  when 'hcg :> IHealthCheckGrain>(clusterClient : IClusterClient, logger : ILogger) = class
        interface IHealthCheck with
            member __.CheckHealthAsync (_ : HealthCheckContext, _ : CancellationToken) =
                try
                    do clusterClient.GetGrain<'hcg>(Guid.Empty).CheckAsync() |> Async.AwaitTask |> Async.RunSynchronously
                    logger.LogInformation <| sprintf "Grain Health Check is healthy : %s" (typeof<'hcg>.Name)
                    HealthCheckResult.Healthy() |> Task.FromResult
                with
                | ex ->
                    let message = sprintf "Health Check Grain %s Reported Failure" (typeof<'hcg>.Name)
                    logger.LogError(ex, message)
                    HealthCheckResult.Unhealthy(message, ex) |> Task.FromResult
    end

    type LocalHealthCheck(clusterClient : IClusterClient, logger : ILogger<LocalHealthCheck>) = class
        inherit GrainHealthCheck<ILocalHealthCheckGrain>(clusterClient, logger)
    end

    type StorageHealthCheck(clusterClient : IClusterClient, logger : ILogger<StorageHealthCheck>) = class
        inherit GrainHealthCheck<IStorageHealthCheckGrain>(clusterClient, logger)
    end

    type ClusterHealthCheck(clusterClient : IClusterClient, logger : ILogger<ClusterHealthCheck>) = class
        interface IHealthCheck with
            member __.CheckHealthAsync (_ : HealthCheckContext, _ : CancellationToken) =
                try
                    clusterClient
                        .GetGrain<IManagementGrain>(0L)
                        .GetHosts()
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> Seq.map(fun kvp -> kvp.Value)
                    |> Seq.filter(fun siloStatus -> siloStatus.IsUnavailable())
                    |> Seq.length
                    |> (fun count ->
                            if (count > 0) then
                                let message = sprintf "%d silo(s) unavailable" count
                                logger.LogInformation <| sprintf "Cluster Health Check is degraded : %s" message
                                HealthCheckResult.Degraded(message) |> Task.FromResult
                            else
                                logger.LogInformation <| sprintf "Silo Health Check is healthy"
                                HealthCheckResult.Healthy() |> Task.FromResult)
                with
                | ex ->
                    let message = sprintf "Health Check Grain %s Reported Failure" (typeof<ClusterHealthCheck>.Name)
                    logger.LogError(ex, message)
                    HealthCheckResult.Unhealthy(message, ex) |> Task.FromResult
    end

    type SiloHealthCheck(participants : IEnumerable<IHealthCheckParticipant>, logger : ILogger<SiloHealthCheck>) = class
        let mutable lastCheckTime = System.DateTime.UtcNow.ToBinary()
        interface IHealthCheck with
            member __.CheckHealthAsync (_ : HealthCheckContext, _ : CancellationToken) =
                let currentTime =
                    Interlocked.Exchange (ref lastCheckTime, System.DateTime.UtcNow.ToBinary())
                    |> DateTime.FromBinary

                logger.LogInformation <| sprintf "Running Silo Health Check at %O" currentTime

                participants
                |> Seq.filter(fun p -> p.CheckHealth currentTime |> (fst >> not))
                |> Seq.length
                |> function
                    | count when (count > 0) ->
                        let message = sprintf "%d services unhealthy." count
                        logger.LogInformation <| sprintf "Silo Health Check is degraded : %s" message
                        HealthCheckResult.Degraded(message)
                    | _ ->
                        logger.LogInformation <| sprintf "Silo Health Check is healthy"
                        HealthCheckResult.Healthy()
                |> Task.FromResult
    end
