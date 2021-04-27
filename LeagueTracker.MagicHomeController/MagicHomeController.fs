module LeagueTracker.MagicHomeController

open MagicHome
open System.Drawing

type Patterns =
    | SevenColorFade = 0x25
    | RedFade = 0x26
    | GreenFade = 0x27
    | BlueFade = 0x28
    | YellowFade = 0x29
    | CyanFade = 0x2a
    | PurpleFade = 0x2b
    | WhiteFade = 0x2c
    | RedGreenFade = 0x2d
    | RedBlueFade = 0x2e
    | GreenBlueFade = 0x2f
    | SevenColorStrobe = 0x30
    | RedStrobe = 0x31
    | GreenStrobe = 0x32
    | BlueStrobe = 0x33
    | YellowStrobe = 0x34
    | CyanStrobe = 0x35
    | PurpleStrobe = 0x36
    | WhiteStrobe = 0x37
    | SevenColorJump = 0x38

/// Connects to the light at the given IP Address
let connectLight (ipAddress: string) : Async<Light> =
    async {
        let light = new Light(ipAddress)
        do! light.ConnectAsync() |> Async.AwaitTask
        light.AutoRefreshEnabled <- true
        return light
    }

let private percentToDelay (speed: int) =
    let s = min 100 speed |> max 0
    30M - (((decimal s) / 100M) * 30M) + 1M |> byte

let private setPower (light: Light) on : Async<unit> =
    async {
        do! light.SetPowerAsync(on) |> Async.AwaitTask
        do! Async.Sleep 500 // TODO: need better solution here, lights are unstable immediately after changing power state
    }

/// Sets the light to perform the specified pattern at the given speed (0-100)
let setPattern (light: Light) (pattern: Patterns) (speed: int) : Async<unit> =
    light.SendAsync(0x61uy, byte pattern, percentToDelay speed, 0x0fuy) |> Async.AwaitTask

/// Sets the light's color to the provided Color
let setColor (light: Light) (color: Color) : Async<unit> =
    light.SetColorAsync(color) |> Async.AwaitTask

/// Sets the light's color to the provided RGB value
let setRgb (light: Light) (r: byte) (g: byte) (b: byte) : Async<unit> =
    light.SetColorAsync(r, g, b) |> Async.AwaitTask

/// Turns the light off
let turnOff light =
    setPower light false

/// Turns the light on
let turnOn light =
    setPower light true

/// Restores the light to the state it was in when first connected
let restore (light: Light) : Async<unit> =
    async {
        do! setColor light light.InitialColor
        do! setPower light light.InitialPowerState
    }
