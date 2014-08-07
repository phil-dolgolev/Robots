module Server
open System
open System.Threading
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
open System.Collections.Generic
open Debug.logOutput

type msg =
|StartTCP
|StopTCP
|StartUDP
|StopUDP


type Server(?UDPip, ?UDPport, ?TCPport) =
    let obs_src = new Event<sbyte[]>()
    let obs = obs_src.Publish
    let obsNext = obs_src.Trigger

    let stopMachine = Array.zeroCreate 4


    let handleRequest (req: byte[]) = 
        let handeledRequest = Array.map sbyte req.[0..3]
        debugWrite "%A" handeledRequest
        handeledRequest |> obsNext

    let agent = 

        let requestIP = "requestIP"
        let UDPport = defaultArg UDPport 3000
        let UDPip = defaultArg UDPip "192.168.0.255"
        let listenerEndPoint = new IPEndPoint(IPAddress.Any, UDPport)
        let senderEndPoint = new IPEndPoint(IPAddress.Parse UDPip, UDPport)
        let UDPserver: Ref<UdpClient option> = ref None


        let TCPport = defaultArg TCPport 9999
        let TCPip: Ref<IPAddress option> = ref None
        let TCPserver: Ref<TcpListener option> = ref None
        let TCPreciever: Ref<TcpClient option> = ref None
        let messageBuf = Array.zeroCreate 1024

        MailboxProcessor.Start (fun inbox ->   
            let UDPSendMacAndIp() =
                debugWrite"Sending...."
                let NIC = NetworkInterface.GetAllNetworkInterfaces().[1]
                let mac = NIC.GetPhysicalAddress().GetAddressBytes()
                let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
                let macAndIp = Array.append mac ip
                (!UDPserver).Value.Send(macAndIp, macAndIp.Length, senderEndPoint) |> ignore
                debugWrite"Sended!"

            let rec UDPrecieveRequest() = 
                async { debugWrite "UDP waiting message..."
                        try
                            let message = (!UDPserver).Value.Receive(ref listenerEndPoint) |> Encoding.ASCII.GetString
                            debugWrite "%s" message
                            if (String.Compare(message, requestIP, true) = 0)
                            then  UDPSendMacAndIp() 
                            return! UDPrecieveRequest()
                        with
                        |_ -> debugWrite "exception in UDPrecieveRequest()"
                            }         
            let rec TCPrecieve() =
                async { debugWrite "TCP message waiting..."
                        try
                            let! count = (!TCPreciever).Value.GetStream().AsyncRead(messageBuf, 0 , messageBuf.Length)
                            if count <> 0
                            then messageBuf |> handleRequest
                                 return! TCPrecieve()
                            else 
                                 stopMachine |> handleRequest
                                 inbox.Post StopTCP
                                 inbox.Post StartUDP
                                 inbox.Post StartTCP
                        with 
                        |_ -> 
                            debugWrite "excpetion in TCPrecieve"
                            stopMachine |> handleRequest
                            inbox.Post StopTCP
                            inbox.Post StartUDP
                            inbox.Post StartTCP
                            }
            let rec handler() =
                async { let! msg = inbox.Receive()
                        match msg with
                        | StartTCP ->
                            debugWrite"Start TCP..."
                            TCPip := Some (Dns.GetHostAddresses(Dns.GetHostName()).[0])
//                            TCPip := Some (IPAddress.Parse "127.0.0.1")
                            TCPserver := Some (new TcpListener( (!TCPip).Value, TCPport))
                            (!TCPserver).Value.Start()
                            debugWrite"TCP Started"
                            try
                                TCPreciever := Some ((!TCPserver).Value.AcceptTcpClient() )
                            with
                            | _ as b -> debugWrite"exception in AcceptTcpClient() %A " b
                                        return! handler()
                            debugWrite"connection established"
                            inbox.Post StopUDP
                            Async.Start (TCPrecieve())
                            return! handler()
                        | StopTCP ->
                            debugWrite "TCP Stoping..."
                            
                            (!TCPreciever).Value.Close()
                            (!TCPserver).Value.Stop()
                            TCPserver := None
                            TCPreciever := None
                            debugWrite "TCP Stoped"
                            debugWrite"inbox.Post StartUDP"
                            //inbox.Post StartUDP
                            return! handler()
                        | StartUDP ->
                            debugWrite "UDP Starting..."
                            UDPserver := Some (new UdpClient(UDPport))
                            debugWrite "UDP Start"
                            Async.Start ( UDPrecieveRequest())
                            return! handler()
                        | StopUDP ->
                            debugWrite "UDP Stoping"
                            (!UDPserver).Value.Close()
                            UDPserver := None
                            debugWrite "UDP Stoped"
                            return! handler()
                        }            
            handler()
                  )
                  
    do agent.Post StartUDP
    do agent.Post StartTCP

    member this.ToObservable() = obs

    interface IDisposable with
        member x.Dispose() = agent.Post StopTCP
                             agent.Post StopUDP
// Возможно может всплыть непредвиденное поведение, если всплывёт, то более безопасно выключать TCP и UDP(возможно уже выключены и не надо ничего делать)                             
