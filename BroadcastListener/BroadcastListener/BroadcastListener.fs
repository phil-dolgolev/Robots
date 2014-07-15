
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
let finishcommand = "complete"
let requestIP = "requestIP"

type UDPServ(?port) =
    let port = defaultArg port 3000
    let mutable messageBuf = Array.create 1024 (byte 0)

    let listener = new UdpClient(port)
    let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)

    let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] // NetworkInterface, надеемся что это то что надо на всех триках
    let mac = NIC.GetPhysicalAddress().GetAddressBytes() // посылать в строке или сразу этот?
    let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
    let macAndIp = Array.append mac ip
    let sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    let senderEndPoint = new IPEndPoint(IPAddress.Any(*IPAddress.Parse "192.168.0.255"*), port)

    let GetMessage() =
        printfn "%s" "Waiting message"
        messageBuf <- listener.Receive(ref listenerEndPoint)
        let msg = Encoding.ASCII.GetString(messageBuf, 0, messageBuf.Length)
        printfn "%A" messageBuf
        printfn "%s"  msg
        msg
    let rec loop() =
        let msg = GetMessage()
        loop()
           
    // потом переписать так же с использованием UDPClient
    do loop()
        

    interface IDisposable with
        member x.Dispose() = ()
       

[<EntryPoint>]
let main argv = 
    use s = new UDPServ()
    0 // return an integer exit code
