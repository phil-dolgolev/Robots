
open System
open System.Net
open System.Net.Sockets
open System.Text

[<EntryPoint>]
let main argv = 
    let port = 3000
    let sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    let ep = new IPEndPoint(IPAddress.Parse "192.168.1.255", port)
    while true do
        let mutable msg = Encoding.ASCII.GetBytes(Console.ReadLine().ToString())
    //while true do
        sender.SendTo(msg, ep) |> ignore
        printfn "%A" "Send"
    0 
