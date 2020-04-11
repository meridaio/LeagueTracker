module EventProcessor

open LeagueEventMonitor.Client.Events
open EventHandler

let processEvent (handler: EventHandler) (e: Event) : Async<unit> =
    async {
        if not (handler.on e) then
            match e with
            | GameStart c -> handler.onGameStart c
            | MinionsSpawning c -> handler.onMinionsSpawning c
            | FirstBrick c -> handler.onFirstBrick c
            | TurretKilled c -> handler.onTurretKilled c
            | InhibKilled c -> handler.onInhibKilled c
            | DragonKill c -> handler.onDragonKill c
            | HeraldKill c -> handler.onHeraldKill c
            | BaronKill c -> handler.onBaronKill c
            | ChampionKill c -> handler.onChampionKill c
            | Multikill c -> handler.onMultikill c
            | Ace c -> handler.onAce c
            | FirstBlood _ -> ()
            | InhibRespawningSoon _ -> ()
    }

let processEvents (handler: EventHandler) (events: Event list) : Async<unit> =
    async {
        match events with
        | [] -> ()
        | _ ->
            printfn "New events: %A" events
            do! List.map (processEvent handler) events |> Async.Sequential |> Async.Ignore
    }
