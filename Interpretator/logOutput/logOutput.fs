module Debug.logOutput

open System
open System.IO

type debugModes =
        | File
        | Console
        | FileAndConsole

let outputFile = "/home/root/log.txt"

let debugMode = Some FileAndConsole
let private monitor = new Object()

let fileWriter = new IO.StreamWriter(outputFile) 
fileWriter.AutoFlush <- true

let debugWrite format = 
    let debugWrite' format =
        match debugMode with
        | Some Console -> Printf.kprintf (fun x ->  lock monitor (fun () -> System.Console.WriteLine x)
                                         ) format
        | Some File -> Printf.kprintf (fun x -> lock monitor (fun () -> fileWriter.WriteLine x )
                                       ) format
        | Some FileAndConsole -> Printf.kprintf( fun x -> lock monitor (fun () -> fileWriter.WriteLine x
                                                                                  Console.WriteLine x 
                                                                                  )
                                               ) format

                                                 

        | None -> Printf.kprintf (fun x -> () ) format
    try 
        debugWrite' format
    with
    | _ as b -> Printf.kprintf (fun x -> File.AppendAllText("log2.txt", "EXCEPTION IN debugWrite" + b.ToString())) format


// Ниже черновики

                    

//Printf.
//
//let debugWrite' x = x |> box |> Unchecked.unbox  