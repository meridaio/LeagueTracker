open EventMonitor
open EventHandler
open Config
open ConsoleHandler
open LEDManager

let mkEventHandler controller ctx =
    ConsoleHandler (ctx, controller) :> EventHandler

[<EntryPoint>]
let main argv =
    let config = loadConfig ()
    let controller = connectToSingle config.ledIP
    programLoop (mkEventHandler controller) |> Async.RunSynchronously
    0
