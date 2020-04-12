module LeagueTracker.Program

open EventMonitor
open Config
open ConsoleHandler
open LEDController.Controller
open System.Drawing
open MagicHomeController.MagicHomeController

[<EntryPoint>]
let main argv =
    async {
        let config = loadConfig ()
        let! light = connectLight config.ledIP
        let controller = LEDController.Controller.LEDController light
        do! programLoop {
            Handler = handleEvents controller
            OnStart = controller.start
            OnEnd = controller.stop
        }
    } |> Async.RunSynchronously
    0
