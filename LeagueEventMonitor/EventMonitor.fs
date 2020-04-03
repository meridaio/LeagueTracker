module EventMonitor

open System

open LeagueEventMonitor.Client.Events
open EventProcessor
open EventHandler
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

    match events with
    | [] -> 0
    | events -> events |> List.map (getEventId >> ((+) 1)) |> List.max


let loop (startingId: int) (callback: Event list -> Async<unit>) : Async<unit> =
    let rec loop' (lastId: int) =
        async {
            let! events = getNextEvents lastId
                
            match events with
            | Some [] ->
                do! Async.Sleep eventPollTimeoutMs
                return! loop' lastId
            | Some e ->
                do! callback e
                do! Async.Sleep eventPollTimeoutMs
                return! loop' <| getNextEventId e
            | _ ->
                return ()
        }
    loop' startingId

let waitForApi () : Async<unit> =
    printfn "Waiting for League API to become ready..."
    let rec waitForApi' () =
        async {
            try
                let! all = getAllStats ()
                all |> Option.map (fun a -> a.ActivePlayer.SummonerName) |> ignore
                printfn "API ready!"
            with
            | _ ->
                do! Async.Sleep readyPollTimeoutMs
                return! waitForApi' ()
        }
    waitForApi' ()

let waitForGame () : Async<unit> =
    printfn "Waiting for game to start..."
    let rec waitForGame' () =
        async {
            let gameRunning =
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                |> Array.exists (fun x -> x.Port = 2999)

            if gameRunning then
                printfn "Game started!"
            else
                do! Async.Sleep gamePollTimeoutMs
                return! waitForGame' ()
        }
    waitForGame' ()

let rec programLoop (mkHandler: EventContext -> EventHandler) =
    monad {
        do! waitForGame ()
        do! waitForApi ()
        let currentTime = DateTime.Now
        let! allInfo = getAllStats ()
        match allInfo with
        | Some allInfo ->
            let startTime = currentTime.AddSeconds(-(float allInfo.GameData.GameTime))
            printfn "Start time: %A" startTime
            printWelcome allInfo
            let handler = mkHandler {
                SummonerName = allInfo.ActivePlayer.SummonerName
            }

            let! events = getEvents ()
            let nextEventId = events |> Option.map getNextEventId |> Option.defaultValue 0
            do! processEvents handler |> loop nextEventId
            return! programLoop mkHandler
        | None ->
            failwith "An error occurred getting stats, aborting"
    }
