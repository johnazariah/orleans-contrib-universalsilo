namespace Orleans.Contrib.UniversalSilo.Utilities

[<AutoOpen>]
module Extensions =
    open System
    type System.String with
        member this.EnumOrDefault<'t when 't : struct and 't :> ValueType and 't : (new : unit -> 't)> defaultValue =
            match Enum.TryParse<'t> this with
            | (true, result) -> result
            | _ -> defaultValue

        member this.StringOrDefault defaultValue =
            if System.String.IsNullOrWhiteSpace(this) then defaultValue else this

        member this.IntOrDefault defaultValue =
            match System.Int32.TryParse this with
            | (true, result) -> result
            | _ -> defaultValue

[<AutoOpen>]
module Networking =
    open System.Net
    open System.Net.NetworkInformation
    open System.Net.Sockets

    let LocalIpAddresses =
        lazy
            NetworkInterface.GetAllNetworkInterfaces()
            |> Seq.filter (fun network -> network.OperationalStatus = OperationalStatus.Up)
            |> Seq.collect (fun network -> network.GetIPProperties().UnicastAddresses)
            |> Seq.map (fun a -> a.Address)
            |> Seq.filter (fun address -> address.AddressFamily = AddressFamily.InterNetwork && not(IPAddress.IsLoopback address))

    let StableIpAddress =
        LocalIpAddresses.Value
        |> Seq.sortBy (fun ip -> ip.ToString ())
        |> Seq.head
