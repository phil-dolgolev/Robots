namespace AsyncTcp
open System.Reactive.Linq
open System
open System.Net
open System.Net.Sockets
open System.Net.NetworkInformation
open System.Text
open Debug.logOutput

type AsyncTcpServer(?port) =
    let port = defaultArg port 2000

    let obs_src = new Event<sbyte[]>()

    let obs = obs_src.Publish
    let obsNext = obs_src.Trigger

    let connectionStatus_src = new Event<bool>()
    let connectionStatus = connectionStatus_src.Publish
    let connectionStatusChanged = connectionStatus_src.Trigger



    let mutable working = false
    let messageBuf = Array.zeroCreate 1024

    let mutable (listener: TcpListener option) = None
    let mutable (ip: IPAddress option) = None

    let handleRequest (req: byte[]) = 
        debugWrite "handleRequest"
        let handeledRequest = Array.map sbyte req.[0..3]
        handeledRequest |> obsNext


    let rec clientLoop(client: TcpClient) = async {
        if client.Connected then
            try
                let! count = client.GetStream().AsyncRead(messageBuf, 0, messageBuf.Length)
                messageBuf |> handleRequest
                if count <> 0 then 
                    return! clientLoop(client)
                else connectionStatusChanged false
            with
                | _  ->     debugWrite "EXCEPTION in AsyncRead"
                            client.Close()
                            connectionStatusChanged false
                            debugWrite "connection false"
        else debugWrite "ELSE"
             connectionStatusChanged false
                 
    }

    let server = async {
        debugWrite "ip address %A" (ip.Value.GetAddressBytes())
        listener.Value.Start()
        let rec loop() = async {
            try
                let client = listener.Value.AcceptTcpClient()
                connectionStatusChanged true
                debugWrite "connection true"
                debugWrite "%s" "new connection established"
                debugWrite "if working..."
                if working then Async.Start(clientLoop client) else client.Close()
                return! loop()
            with |_ -> debugWrite "EXCEPTION IN SERVER"
        }
        Async.Start( loop() )
    }
        
    do debugWrite "%s" "TCP server initial"
    do connectionStatusChanged false


    member this.ToObservable() = obs
    member val ConnectionStatus = connectionStatus

    member this.serverStart() = match listener with
                                    | None -> debugWrite "TCP start"
                                              working <- true
                                              ip <- Some (Dns.GetHostAddresses(Dns.GetHostName()).[0])
//                                              ip <- Some (IPAddress.Parse "127.0.0.1") // отладка на компе
                                              listener <- Some (new TcpListener(ip.Value, port))
                                              Async.Start( server )
                                              
                                    | Some _ -> debugWrite "TCP alredy started"
                                                ()

    member this.serverStop() = match listener with
                                    | None -> debugWrite"TCP already stoped"
                                              ()
                                    | Some serv -> debugWrite "TCP begining stop"
                                                   working <- false
                                                   listener.Value.Stop()
                                                   listener <- None
                                                   debugWrite "TCP successfully stoped"

    interface IDisposable with
        member x.Dispose() = working <- false

