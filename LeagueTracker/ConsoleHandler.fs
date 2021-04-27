module LeagueTracker.ConsoleHandler

open LEDController.Controller
open LeagueTracker.EventMonitor.Client.Events
open LeagueTracker.EventMonitor.Client.LeagueClient
open LeagueTracker.EventMonitor.EventMonitor

let handleEvents (controller: LEDController) (ctx: EventContext) (e: Event) =
    match e with
    | ChampionKill c ->
        match c.VictimName, c.KillerName, c.Assisters with
        | v, _, _ when v = ctx.SummonerName ->
            printfn "You died!! lol!!"
            async {
                let! playerList = getPlayerList ()
                match playerList with
                | Ok p -> 
                    let player = p |> Array.find (fun x -> x.SummonerName = ctx.SummonerName)
                    controller.enqueueEvent <| Die (int (player.RespawnTimer * 1000M))
                | Error e ->
                    printfn "Error: unable to get player respawn timer: %A" e
            } |> Async.Start
        | v, k, a when k = ctx.SummonerName && Seq.isEmpty a ->
            controller.enqueueEvent Kill
            printfn "You killed %s all by yourself!" v
        | v, k, a when k = ctx.SummonerName ->
            controller.enqueueEvent Kill
            printfn "You killed %s with some help from %s" v (String.concat ", " a)
        | v, k, a when a |> Seq.contains ctx.SummonerName ->
            printfn "You assisted %s killing %s" k v
        | _ -> ()
    | Multikill k ->
        if k.KillerName = ctx.SummonerName then
            controller.enqueueEvent <| Multi k.KillStreak
    | GameStart _ ->
        printfn "GLHF!"
    | BaronKill b ->
        controller.enqueueEvent <| Baron b.Stolen
    | DragonKill d ->
        controller.enqueueEvent <|
            match d.DragonType with
            | Earth -> EarthDrag
            | Air -> AirDrag
            | Water -> WaterDrag
            | Fire -> FireDrag
            | Elder -> ElderDrag
    | GameEnd g ->
        printfn "Game ended: %A" g.Result
    | _ ->
        printfn "Something else happened! (%A)" e
