namespace AsyncUdp
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
open System.Collections.Generic
open Debug.logOutput

type AsyncUdpServer(ip, port) = 

    let requestIP = "requestIP"


    let mutable messageBuf = Array.zeroCreate 1024
    let mutable working = false

    let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)
    let senderEndPoint = new IPEndPoint(IPAddress.Parse ip, port)

    let test_src = new Event<bool>()
    let test = test_src.Publish
    let testst = test_src.Trigger
    
    let mutable (Udp : UdpClient option) =  None  

    let GetMessage() =
        try
            messageBuf <- Udp.Value.Receive(ref listenerEndPoint)
        with | _  -> debugWrite "%s" "Recieve break by close, nothing serious" 
        // вылетает эксепшн, из-за того что в другом потоке закрывается UDPClient, можно списать на косяк архитектуры
        // но на работоспособность не влияет
        Encoding.ASCII.GetString(messageBuf, 0, messageBuf.Length)

    let sendMacAndIp() = 
        let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] 
        let mac = NIC.GetPhysicalAddress().GetAddressBytes()
        let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
        let macAndIp = Array.append mac ip
        debugWrite "%s %A" "Sending macAndIp" macAndIp
        Udp.Value.Send(macAndIp, macAndIp.Length, senderEndPoint) |> ignore
        debugWrite "%s" "macAndIp Sended"
        
    let rec loop() = async {
        if working then 
                        debugWrite "waiting message by UDP"
                        let msg = GetMessage()
                        debugWrite "%s recieved by UDP" msg
                        testst true
                        if (String.Compare(msg, requestIP, true) = 0) then 
                            try
                                sendMacAndIp()
                            with
                            | _ -> printfn"EXCEPTION IN sendMacAndIp()" 
                            // вроде как пытается отослать когда вырубили уже UDP, пофиксить в будущем
                        else ()
                        return! loop()
    }
        
    member x.ServerStart() = match Udp with
                             | None -> debugWrite " UDP start"
                                       Udp <- Some (new UdpClient(port))
                                       working <- true
                                       Async.Start( loop() )
                             | _ -> debugWrite "UDP already running"
                                    ()

    member x.ServerStop() =  match Udp with
                             | None -> debugWrite "useless trying stop UDP server"
                             | Some server -> debugWrite "UDP begin stoped"
                                              working <- false
                                              try
                                                server.Close()
                                              with
                                                | _ -> debugWrite "EXCEPTION in UdpClient.Close()"
                                              Udp <- None
                                              debugWrite "UDP successfully stoped"
    member val obs = test
                           
    interface IDisposable with
        member x.Dispose() = working <- false
        
