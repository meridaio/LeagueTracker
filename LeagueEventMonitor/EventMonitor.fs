module EventMonitor

open System

open Events
open EventProcessor
open EventHandler
open LeagueClient
open System.Net.NetworkInformation

let printWelcome (allInfo: AllData) =
    printfn "Hello, %s! Welcome to League Tracker!" allInfo.ActivePlayer.SummonerName

open TypedClient
let loop (callback: Event list -> Async<unit>) : Async<unit> =
    let rec loop' (totalEvents: Event list option) =
        async {
            let! events = getEvents ()
            let processEvents e =
                async {
                    do! callback e
                    do! Async.Sleep 500
                    return! loop' events
                }
  
            do! match events, totalEvents with
                | Some e, Some p -> List.except p e |> processEvents
                | Some e, None   -> e |> processEvents
                | _              -> async { () }
            
            (*let! events =
                match totalEvents with
                | Some p ->
                    p |> List.map (fun x -> x.EventId) |> List.max |> (+) 1 |> getNextEvents
                | None ->
                    getEvents ()
                
            match events, totalEvents with
            | Some e, Some t ->
                do! List.map parseEvent e |> callback
                do! Async.Sleep 500
                return! loop' <| Some (t @ e)
            | Some e, None ->
                do! callback e
                do! Async.Sleep 500
                return! loop' events
            | _ ->
                return ()
            *)
        }
    loop' None

let waitForApi () : Async<unit> =
    printfn "Waiting for League API to become ready..."
    let rec waitForApi' () =
        async {
            try
                let! all = getAllStats ()
                all.ActivePlayer.SummonerName |> ignore
                printfn "API ready!"
            with
            | _ ->
                do! Async.Sleep 1000
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
                do! Async.Sleep 5000
                return! waitForGame' ()
        }
    waitForGame' ()

let rec programLoop (mkHandler: EventContext -> EventHandler) : Async<unit> =
    async {
        do! waitForGame ()
        do! waitForApi ()
        let currentTime = DateTime.Now
        let! allInfo = getAllStats ()
        let startTime = currentTime.AddSeconds(-(float allInfo.GameData.GameTime))
        printfn "Start time: %A" startTime
        printWelcome allInfo
        let handler = mkHandler {
            SummonerName = allInfo.ActivePlayer.SummonerName
        }
        do! processEvents handler |> loop
        return! programLoop mkHandler
    }
