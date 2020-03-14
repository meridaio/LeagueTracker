open System

open EventMonitor
open EventHandler
open ConsoleHandler

let mkEventHandler ctx =
    ConsoleHandler ctx :> EventHandler

[<EntryPoint>]
let main argv =
    programLoop mkEventHandler |> Async.RunSynchronously
    0
