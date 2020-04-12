module EventMonitor

open System

open LeagueEventMonitor.Client.Events
open LeagueEventMonitor.Client.LeagueClient
open System.Net.NetworkInformation
open FSharpPlus
open FSharpPlus.Data

let printWelcome (allInfo: AllData) =
    printfn "Hello, %s! Welcome to League Tracker!" allInfo.ActivePlayer.SummonerName

[<Literal>]
let eventPollTimeoutMs = 250

[<Literal>]
let readyPollTimeoutMs = 1000

[<Literal>]
let gamePollTimeoutMs = 5000

type EventContext = {
    SummonerName: string
}

type EventHandler = {
    Handler: EventContext -> Event -> unit
    OnStart: unit -> Async<unit>
    OnEnd: unit -> Async<unit>
}

let processEvents (handler: Event -> unit) (events: Event list) : unit =
    match events with
    | [] -> ()
    | _ ->
        printfn "New events: %A" events
        List.map handler events |> ignore

let getNextEventId (events: Event list) : int =
    let getEventId = function
        | GameStart c -> c.Data.EventID
        | MinionsSpawning c -> c.Data.EventID
        | FirstBrick c -> c.Data.EventID
        | TurretKilled c -> c.Data.EventID
        | InhibKilled c -> c.Data.EventID
        | DragonKill c -> c.Data.EventID
        | HeraldKill c -> c.Data.EventID
        | BaronKill c -> c.Data.EventID
        | ChampionKill c -> c.Data.EventID
        | Multikill c -> c.Data.EventID
        | Ace c -> c.Data.EventID
        | FirstBlood c -> c.Data.EventID
        | InhibRespawningSoon c -> c.Data.EventID
        | InhibRespawned c -> c.Data.EventID
        | GameEnd c -> c.Data.EventID

    match events with
    | [] -> 0
    | events -> events |> List.map (getEventId >> ((+) 1)) |> List.max


let loop (startingId: int) (callback: Event list -> unit) : Async<unit> =
    let rec loop' (lastId: int) =
        async {
            let! events = getNextEvents lastId
                
            match events with
            | Ok [] ->
                do! Async.Sleep eventPollTimeoutMs
                return! loop' lastId
            | Ok e ->
                callback e
                do! Async.Sleep eventPollTimeoutMs
                return! loop' <| getNextEventId e
            | Error e ->
                printfn "Error getting events: %A" e
                return ()
        }
    loop' startingId

let waitForApi () : Async<unit> =
    printfn "Waiting for League API to become ready..."
    let rec waitForApi' () =
        async {
            try
                let! all = getAllStats ()
                all |> Result.map (fun a -> a.ActivePlayer.SummonerName) |> function Ok _ -> () | Error e -> raise e
                printfn "API ready!"
            with
            | _ ->
                do! Async.Sleep readyPollTimeoutMs
                return! waitForApi' ()
        }
    waitForApi' ()

let gameIsRunning () =
    IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
    |> Array.exists (fun x -> x.Port = 2999)

let waitForGameStart () : Async<unit> =
    printfn "Waiting for game to start..."
    let rec waitForGame' () =
        async {
            if gameIsRunning () then
                printfn "Game started!"
            else
                do! Async.Sleep gamePollTimeoutMs
                return! waitForGame' ()
        }
    waitForGame' ()

let rec waitForGameShutdown () : Async<unit> =
    async {
        if not <| gameIsRunning () then
            printfn "Game ended!"
        else
            do! Async.Sleep gamePollTimeoutMs
            return! waitForGameShutdown ()
    }

let rec programLoop (handler: EventHandler) =
    async {
        do! waitForGameStart ()
        do! waitForApi ()
        let currentTime = DateTime.Now
        let! allInfo = getAllStats ()
        match allInfo with
        | Ok allInfo ->
            let startTime = currentTime.AddSeconds(-(float allInfo.GameData.GameTime))
            printfn "Start time: %A" startTime
            printWelcome allInfo
            let handleEvent = handler.Handler {
                SummonerName = allInfo.ActivePlayer.SummonerName
            }

            let! events = getEvents ()
            let nextEventId = events |> Result.map getNextEventId |> function Ok x -> x | _ -> 0
            do! handler.OnStart ()
            do! processEvents handleEvent |> loop nextEventId
            do! waitForGameShutdown ()
            do! handler.OnEnd ()
            return! programLoop handler
        | Error e ->
            printfn "An error occurred getting stats: %A" e
            return! programLoop handler
    }
