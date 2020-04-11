open System
open MagicHomeController.MagicHomeController
open MagicHomeController.Patterns
open System.Drawing

[<EntryPoint>]
let main argv =
    async {
        let! light = connectLight "192.168.0.183"
        if not light.IsOn then do! turnOn light
        do! setPattern light Patterns.PurpleFade 90
        do! Async.Sleep 5000
        do! setColor light Color.Purple
        do! Async.Sleep 5000
        do! restore light
    } |> Async.RunSynchronously
    0 // return an integer exit code
