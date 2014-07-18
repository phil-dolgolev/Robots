﻿open Trik
open System
open System.IO
open Server
open Debug.logOutput

module interpret = 
    let maxPositionOfKicker = -100
    let minPositionOfKicker = 100
    let stop = 0
    type Interpretator() =
        let model = new Model()
        let motor1 = model.Motor.["JM1"]
        let motor2 = model.Motor.["JM2"]
        let servo1 = model.Servo.["JE1"]
        let led = model.Led
        // скорее всего надо добавить задержку
        let kick() = async {
            servo1.SetPower ( maxPositionOfKicker )
            System.Threading.Thread.Sleep 300
            servo1.SetPower (minPositionOfKicker)
            System.Threading.Thread.Sleep 300
            servo1.Zero()
            }

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
            motor1.SetPower (int command.[0])
            motor2.SetPower (int command.[1])
            if command.[2] = 1y then Async.Start ( kick() )
                else ()
            setColor command.[3]


        member this.Start(events: IObservable<sbyte[]>) =
            events.Subscribe interpret

        interface IDisposable with
            member this.Dispose() = (model :> IDisposable).Dispose()
    

    [<EntryPoint>]

    let main _ = 
        try
            Helpers.I2C.Init "/dev/i2c-2" 0x48 1

            use server = new Server("192.168.1.255", 3000, 2000) 

            let interpr = new Interpretator()
            use sbs = interpr.Start (server.ToObservable())
            debugWrite "interpretator started"
            Console.ReadKey() |> ignore
        with | _ as b -> debugWrite "EXCEPTION IN MAIN %A" b
        0
