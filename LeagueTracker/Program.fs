﻿module LeagueTracker.Program

open EventMonitor
open EventHandler
open Config
open ConsoleHandler
open LEDController.Controller
open System.Drawing
open MagicHomeController.MagicHomeController

let mkEventHandler controller ctx =
    ConsoleHandler (ctx, controller) :> EventHandler

[<EntryPoint>]
let main argv =
    async {
        let config = loadConfig ()
        let! light = connectLight config.ledIP
        if not light.IsOn then do! turnOn light
        do! setColor light Color.Green
        let controller = LEDController.Controller.LEDController light
        do! programLoop (mkEventHandler controller)
    } |> Async.RunSynchronously
    0
