open Trik
open System
open Server
module interpret = 
//    type Interpretator() =
//        let model = new Model()
//        let motor1 = model.Motor.["JM1"]
//        let motor2 = model.Motor.["JM2"]
//        let servo1 = model.Servo.["JE1"]
//        let led = model.Led
//        let interpret(command: sbyte[])=
//            printfn "set power on JM1 %A" command.[0]
//            motor1.SetPower (int command.[0])
//            printfn "set power on JM2 %A" command.[1]
//            motor2.SetPower (int command.[1])
//            printfn "set power on JE1 %A" command.[2]
//            servo1.SetPower (int command.[2])
//
//        member this.Start(events: IEvent<sbyte[]>) =
//            events.Subscribe interpret
//
//        interface IDisposable with
//            member this.Dispose() = (model :> IDisposable).Dispose()
//                                    
    type Interpretator() =
        let model = new Model()
        let motorLeft = model.Motor.["JM1"]
        let motorRight = model.Motor.["JM2"]
        let interpret(command: byte[]) =
            //printfn "%A %A" command.[0] command.[1]
            if command.[0] = 1uy then motorLeft.SetPower (int command.[1])
            elif command.[0] = 2uy then motorRight.SetPower (int command.[1])
            else motorLeft.SetPower 0
                 motorRight.SetPower 0

        member this.Start(events(*: IEvent<byte[]>*): IObservable<byte[]>) =
            events.Subscribe interpret


    [<EntryPoint>]

    let main argv = 

        Helpers.I2C.Init "/dev/i2c-2" 0x48 1
        use server = new Server("192.168.1.255", 3000, 2000) 

        let interpr = new Interpretator()
        interpr.Start (server.Observable) |> ignore

        Console.ReadKey() |> ignore
        0
