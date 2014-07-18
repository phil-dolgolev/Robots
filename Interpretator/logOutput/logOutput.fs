module Debug.logOutput

open System
open System.IO

type debugMode =
        | File
        | Console
        | FileAndConsole

let outputFile = "log.txt"
let private parameter = Some FileAndConsole
let private monitor = new Object()

let debugWrite format = 
    let debugWrite' format =
        match parameter with
        | Some Console -> Printf.kprintf (fun x ->  lock monitor (fun () -> System.Console.WriteLine x)
                                         ) format
        | Some File -> Printf.kprintf (fun x -> lock monitor (fun () -> File.AppendAllText("log.txt", (x+"\n")))
                                       ) format
        | Some FileAndConsole -> Printf.kprintf( fun x -> lock monitor (fun () -> File.AppendAllText("log.txt", (x+"\n"))
                                                                                  Console.WriteLine x )
                                               ) format
                                               // Если будут проблемы с производительностью,
                                               // заменить AppendAllText на более легковесный вариант
                                                 

        | None -> Printf.kprintf (fun x -> () ) format
    debugWrite' format


// Ниже черновики

                    

//Printf.
//
//let debugWrite' x = x |> box |> Unchecked.unbox  