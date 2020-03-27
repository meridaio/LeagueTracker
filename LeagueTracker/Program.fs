open System

open EventMonitor
open EventHandler
open ConsoleHandler
open LeagueEventMonitor.Client.LeagueClient

open MagicHome

let mkEventHandler light ctx =
    ConsoleHandler (ctx, light) :> EventHandler

[<EntryPoint>]
let main argv =
    let light = Light()
    light.AutoRefreshEnabled <- true
    light.ConnectAsync("192.168.0.183") |> Async.AwaitTask |> Async.RunSynchronously
    light.TurnOffAsync() |> Async.AwaitTask |> Async.RunSynchronously
    light.TurnOnAsync() |> Async.AwaitTask |> Async.RunSynchronously
    light.SetColorAsync(System.Drawing.Color.Green) |> Async.AwaitTask |> Async.RunSynchronously
    programLoop (mkEventHandler light) |> Async.RunSynchronously
    light.TurnOffAsync() |> Async.AwaitTask |> Async.RunSynchronously
    0
