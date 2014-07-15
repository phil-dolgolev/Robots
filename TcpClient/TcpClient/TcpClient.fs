open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Collections.Generic

type TCPClient(?ipAddr, ?port) =
    let port = defaultArg port 2000
    let ipAddr = defaultArg ipAddr "192.168.1.1"
    let mutable working = false
    let mutable sender = new TcpClient(ipAddr, port)
    let rec client () = 
        if (sender.Connected)&&(working) then
            printfn "%A" "Enter the command"
            let command = Encoding.ASCII.GetBytes (System.Console.ReadLine().ToString())  
            sender.GetStream().Write(command, 0, command.Length)
        else sender <- new TcpClient(ipAddr, port)
        client ()

    do working <- true
    do client ()

    interface IDisposable with
        member x.Dispose() = working <- false


[<EntryPoint>]
let main argv = 
    let sender = new TCPClient("192.168.0.101", 2000)
    Console.ReadKey() |> ignore
    0 
