module LEDManager

open System.Drawing
open System.Threading

open Colors
open MagicHome
open LeagueEventMonitor.Client.Events

type LEDController (lg: LightGroup) =
    let setColor (color: Color) =
        lg.SetColor(colorToRgb color)

    let setRgb (r: int) (g: int) (b: int) =
        lg.SetColor(RGB(byte r, byte g, byte b))

    new (light: Light) =
        LEDController(LightGroup light)

    member __.death (timeout: int) =
        async {
            setColor Color.Red
            do! Async.Sleep timeout
            setColor Color.Green
        }

    member __.baronKill () =
        async {
            lg.SetPresetPatterm(PresetPattern.PurpleGradualChange, 150uy)
            do! Async.Sleep 2000
            setColor Color.Green
        }

    member __.baronSteal () =
        async {
            lg.SetPresetPatterm(PresetPattern.PurpleStrobeFlash, 100uy)
            do! Async.Sleep 5000
            setColor Color.Green
        }

    member __.dragonKill (dragonType: DragonType) =
        async {
            match dragonType with
            | Earth ->
                setRgb 189 86 99
                do! Async.Sleep 2000
                setColor Color.Green
            | Air ->
                setColor Color.White
                do! Async.Sleep 2000
                setColor Color.Green
            | Water ->
                setColor Color.DarkCyan
                do! Async.Sleep 2000
                setColor Color.Green
            | Fire ->
                setRgb 255 53 0
                do! Async.Sleep 2000
                setColor Color.Green
            | Elder ->
                lg.SetPresetPatterm(PresetPattern.WhiteGradualChange, 150uy)
                do! Async.Sleep 5000
                setColor Color.Green
        }

    member __.dragonSteal () =
        async {
            // TODO: Dragon Steal
            printfn "TODO: Dragon Steal"
        }

let connectToAll () : LEDController =
    let lg = Light.Discover() |> LightGroup
    lg.TurnOn()
    lg.SetColor(colorToRgb Color.Green)
    LEDController lg

let connectToSingle (name: string) : LEDController =
    let light = Light(name)
    Thread.Sleep 2000
    light.Refresh()
    Thread.Sleep 2000
    if not light.IsOn then light.TurnOn()
    Thread.Sleep 2000
    light.SetColor(colorToRgb Color.Green)
    LEDController light

