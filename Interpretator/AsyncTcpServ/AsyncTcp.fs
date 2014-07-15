namespace AsyncTcp
open System.Reactive.Linq
open System
open System.Net
open System.Net.Sockets
open System.Net.NetworkInformation
open System.Text

type AsyncTcpServer(?port) =
    let port = defaultArg port 2000

//    let obs_src = new Event<sbyte[]>()
    let obs_src = new Event<byte[]>()
    let obs = obs_src.Publish
    let obsNext = obs_src.Trigger

    let connectionStatus_src = new Event<bool>()
    let connectionStatus = connectionStatus_src.Publish
    let connectionStatusChanged = connectionStatus_src.Trigger



    let mutable working = false
    let mutable messageBuf = Array.create 1024 (byte 0)

    let mutable (listener: TcpListener option) = None
    let mutable (ip: IPAddress option) = None

    let handleRequest (req: byte[]) = 
        //printfn "%A" req.Length
//        if req.[4] = byte 48 then let handeledRequest = [|sbyte req.[0]; 
//                                                          sbyte req.[1]; 
//                                                          sbyte req.[2]; 
//                                                          sbyte req.[3]|]
//                                  handeledRequest |> obsNext
//        else printfn "Warning! Unknown command"
        [|req.[0]; req.[1]|] |> obsNext


    let rec clientLoop(client: TcpClient) = async {
        //printfn "ClientLoop"
        if client.Connected then
            try
                let! count = client.GetStream().AsyncRead(messageBuf, 0, messageBuf.Length)
                messageBuf |> handleRequest 
                return! clientLoop(client)
            with
                | _ -> printfn "EXCEPTION in AsyncRead"
                       client.Close()
                       connectionStatusChanged false
                       printfn "connection false"
            else connectionStatusChanged false
                 
                       
    }

    let server = async {
        printfn "ip address %A" (ip.Value.GetAddressBytes())
        listener.Value.Start()
        let rec loop() = async {
            let client = listener.Value.AcceptTcpClient()
            connectionStatusChanged true
            printfn "connection true"
            printfn "%s" "new connection established"
            try
                printfn "if working..."
                if working then Async.Start(clientLoop client) else client.Close()
            with
                |_ -> listener.Value.Stop(); new AsyncTcpServer(port) |> ignore; printfn"Exception in loop"// проверить на необходимость

            return! loop()
        }
        Async.Start( loop() )
    }
        
    do printfn "%s" "TCP server initial"


    member val Observable = obs.DistinctUntilChanged()
    member val ConnectionStatus = connectionStatus.DistinctUntilChanged()

    member this.serverStart() = match listener with
                                    | None -> printfn "TCP start"
                                              working <- true
                                              ip <- Some (Dns.GetHostAddresses(Dns.GetHostName()).[0])
                                              listener <- Some (new TcpListener(ip.Value, port))
                                              Async.Start( server )
                                              
                                    | Some _ -> printfn "TCP alredy started"
                                                ()

    member this.serverStop() = match listener with
                                    | None -> printfn"TCP already stoped"
                                              ()
                                    | Some serv -> printfn "TCP begining stop"
                                                   working <- false
                                                   listener.Value.Stop()
                                                   listener <- None
                                                   printfn "TCP successfully stoped"

    interface IDisposable with
        member x.Dispose() = working <- false

