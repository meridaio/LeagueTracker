module LEDController.Controller

open System.Drawing
open MagicHome
open MagicHomeController.MagicHomeController
open System.Collections.Concurrent

type LEDEvent =
    | Die of timeoutMs: int
    | Res
    | Kill
    | Assist
    | EarthDrag
    | WaterDrag
    | AirDrag
    | FireDrag
    | ElderDrag
    | Baron of stolen: bool

type private ColorAction =
    | C of Color
    | R of r: byte * g: byte * b: byte
    | P of pattern: Patterns * speed: int

type private ControllerAction = {
    timeMs: int
    action: ColorAction
}

type private ReduceResult =
    | CA of ControllerAction
    | Noop

type LEDController (light: Light) =
    let mutable dead = false

    let defaultColor () =
        async {
            if dead then
                do! setColor light Color.Red
            else
                do! setColor light Color.Green
        }

    member private this.message = MailboxProcessor.Start <| fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                let! res = this.reduce msg
                match res with
                | CA ca ->
                    match ca.action with
                    | C color ->
                        do! setColor light color
                    | R (r, g, b) ->
                        do! setRgb light r g b
                    | P (pattern, speed) ->
                        do! setPattern light pattern speed
                    do! Async.Sleep ca.timeMs
                | Noop -> ()

                do! defaultColor ()
                return! loop ()
            }
        loop ()

    /// Takes in an event and updates LED state. If many events happen in a row,
    /// this function will allow each event to play out its animation in full
    /// (apart from deaths)
    member private this.reduce (event: LEDEvent) : Async<ReduceResult> =
        async {
            match event with
            | Die timeoutMs ->
                dead <- true
                async {
                    do! Async.Sleep timeoutMs
                    this.enqueueEvent Res
                } |> Async.Start
                return Noop
            | Res ->
                dead <- false
                return Noop
            | Kill ->
                return CA {
                    timeMs = 1000
                    action = C Color.Yellow
                }
            | Assist ->
                return Noop
            | EarthDrag ->
                return CA {
                    timeMs = 2000
                    action = R (189uy, 86uy, 99uy)
                }
            | WaterDrag ->
                return CA {
                    timeMs = 2000
                    action = C Color.DarkCyan
                }
            | AirDrag ->
                return CA {
                    timeMs = 2000
                    action = C Color.White
                }
            | FireDrag ->
                return CA {
                    timeMs = 2000
                    action = R (255uy, 53uy, 0uy)
                }
            | ElderDrag ->
                return CA {
                    timeMs = 5000
                    action = P (Patterns.WhiteFade, 90)
                }
            | Baron stolen ->
                if stolen then
                    return CA {
                        timeMs = 5000
                        action = P (Patterns.PurpleStrobe, 95)
                    }
                else
                    return CA {
                        timeMs = 2000
                        action = P (Patterns.PurpleFade, 90)
                    }
        }

    member this.enqueueEvent (e: LEDEvent) =
        this.message.Post e
