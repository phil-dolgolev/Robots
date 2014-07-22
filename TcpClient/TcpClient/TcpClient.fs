open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Collections.Generic

type TCPClient(?ipAddr, ?port) =
    let parse (str: string) : byte[] =
       str.Split(';') |> Array.map Byte.Parse 
    let port = defaultArg port 2000
    let ipAddr = defaultArg ipAddr "192.168.1.1"
    let mutable working = false
    let mutable sender = new TcpClient(ipAddr, port)
    let rec client () = 
        if (sender.Connected)&&(working) then
            printfn "%A" "Enter the command"
            let command' = System.Console.ReadLine().ToString()
            if ((String.Compare (command',"exit" )) = 0 )
            then sender.Close(); working <- false
            else
                 //let command = Encoding.ASCII.GetBytes (command')  
                 let command = command' |> parse
                 printfn "%A" command
                 sender.GetStream().Write(command, 0, command.Length)
            client ()

    do working <- true
    do client ()

    interface IDisposable with
        member x.Dispose() = working <- false


[<EntryPoint>]
let main argv = 
//    let sender = new TCPClient("127.0.0.1", 2000) // отладка на компе
    let sender = new TCPClient("192.168.1.1", 2000) 
    Console.ReadKey() |> ignore
    0 

// отправлять в формате "motor1;motor2;kick;led" без кавычек, где motor1, motor2 это скорости на двигатели(от -100 до 100),
// kick - пинать (1), led (0- оставить предыдущее состояние, 1 - выключить, 2 - зелёный, 3 - оранжевый, 4 - красный