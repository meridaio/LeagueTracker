module LeagueTracker.Program

open LeagueTracker.EventMonitor.EventMonitor
open Config
open ConsoleHandler
open LeagueTracker.MagicHomeController

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
