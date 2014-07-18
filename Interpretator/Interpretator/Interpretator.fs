open Trik
open System
open System.IO
open Server
open Debug.logOutput

module interpret = 
    let maxPositionOfKicker = 100
    let minPositionOfKicker = -100
    type Interpretator() =
        let model = new Model()
        let motor1 = model.Motor.["JM1"]
        let motor2 = model.Motor.["JM2"]
        let servo1 = model.Servo.["JE1"]
        let led = model.Led
        // скорее всего надо добавить задержку
        let kick() =
            servo1.SetPower ( maxPositionOfKicker )
            servo1.SetPower (minPositionOfKicker)

        let setColor color =
            match color with
            | 0y -> ()
            | 1y -> led.SetColor LedColor.Off; debugWrite "Off"
            | 2y -> led.SetColor LedColor.Green; debugWrite "Green"
            | 3y -> led.SetColor LedColor.Orange; debugWrite "Orange"
            | 4y -> led.SetColor LedColor.Red; debugWrite "Red"
            | _ as b -> debugWrite "expected 0..4 color, but found %d" b
            

        let interpret(command: sbyte[])=
            debugWrite "Interpret"
            //printfn "set power on JM1 %A" command.[0]
            motor1.SetPower (int command.[0])
            //printfn "set power on JM2 %A" command.[1]
            motor2.SetPower (int command.[1])
            if command.[2] = 1y then kick()
                else ()
            setColor command.[3]


        member this.Start(events: IObservable<sbyte[]>) =
            events.Subscribe interpret

        interface IDisposable with
            member this.Dispose() = (model :> IDisposable).Dispose()
    
    
// Special for Nastya                            
//    type Interpretator() =
//        let model = new Model()
//        let motorLeft = model.Motor.["JM1"]
//        let motorRight = model.Motor.["JM2"]
//        let interpret(command: byte[]) =
//            //printfn "%A %A" command.[0] command.[1]
//            if command.[0] = 1uy then motorLeft.SetPower (int command.[1])
//            elif command.[0] = 2uy then motorRight.SetPower (int command.[1])
//            else motorLeft.SetPower 0
//                 motorRight.SetPower 0
//
//        member this.Start(events(*: IEvent<byte[]>*): IObservable<byte[]>) =
//            events.Subscribe interpret


    [<EntryPoint>]

    let main _ = 
        try
            Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    //        debugWrite "%d" 1024

            use server = new Server("192.168.1.255", 3000, 2000) 
    //
            let interpr = new Interpretator()
            use sbs = interpr.Start (server.ToObservable())

            Console.ReadKey() |> ignore
        with | _ as b -> debugWrite "EXCEPTION IN MAIN %A" b
        0
