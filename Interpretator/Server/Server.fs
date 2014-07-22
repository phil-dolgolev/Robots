namespace Server
open System.Reactive.Linq
open System
open AsyncUdp
open AsyncTcp
open Debug.logOutput

type Server(?udpIpMask, ?portForUdp, ?portForTcp) = 

    let udpIpMask = defaultArg udpIpMask "192.168.0.255"
    let portForUdp = defaultArg portForUdp 3000
    let portForTcp = defaultArg portForTcp 2000
    let UDPserver = new AsyncUdpServer(udpIpMask, portForUdp)
    let TCPserver = new AsyncTcpServer(portForTcp)

    let handler x =

        if x then UDPserver.ServerStop()
        else TCPserver.serverStop()
             TCPserver.serverStart()
             UDPserver.ServerStart()

    do TCPserver.ConnectionStatus.Add ( handler )
       UDPserver.ServerStart()
       TCPserver.serverStart()
    
    member x.ToObservable() = TCPserver.ToObservable()

    interface IDisposable with
        member x.Dispose() = 
            (UDPserver:>IDisposable).Dispose()
            (TCPserver:>IDisposable).Dispose()