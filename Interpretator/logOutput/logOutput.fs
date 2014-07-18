module Debug.logOutput

open System
open System.IO

type debugMode =
        | File
        | Console
        | FileAndConsole

let outputFile = "/home/root/log.txt"
let private parameter = Some Console
let private monitor = new Object()
let fileWriter = new IO.StreamWriter(outputFile) //File.Create(outputFile)
fileWriter.AutoFlush <- true
let debugWrite format = 
    let debugWrite' format =
        match parameter with
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