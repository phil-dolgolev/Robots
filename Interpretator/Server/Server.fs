namespace Server
open System.Reactive.Linq
open System
open AsyncUdp
open AsyncTcp

type Server(?udpIpMask, ?portForUdp, ?portForTcp) = 

    let udpIpMask = defaultArg udpIpMask "192.168.0.255"
    let portForUdp = defaultArg portForUdp 3000
    let portForTcp = defaultArg portForTcp 2000
    let UDPserver = new AsyncUdpServer(udpIpMask, portForUdp)
    let TCPserver = new AsyncTcpServer(portForTcp)

    let handler x =
        printfn "handler"
        if x then UDPserver.ServerStop()
        else UDPserver.ServerStart()

    do TCPserver.ConnectionStatus.Subscribe ( handler ) |> ignore
    do UDPserver.ServerStart()
    do TCPserver.serverStart()
    
    member val Observable = TCPserver.Observable

    interface IDisposable with
        member x.Dispose() = ()