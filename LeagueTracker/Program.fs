open System

open LeagueClient
open System.Net.NetworkInformation

let printWelcome (allInfo: AllJson.Root) =
    printfn "Hello, %s! Welcome to League Tracker!" allInfo.ActivePlayer.SummonerName
    printfn "Your current score is: %A" (Array.find (fun (x: AllJson.AllPlayer) -> x.SummonerName = allInfo.ActivePlayer.SummonerName) allInfo.AllPlayers).SummonerName
    printfn "All events: %A" allInfo.Events.Events

let rec loop (name: string) (callback: EventJson.Root -> Async<unit>) =
    async {
        let! events = getEvents ()
        match events with
        | Some e -> 
            do! callback e
            do! Async.Sleep 500
            return! loop name callback
        | None ->
            return ()
    }

let rec waitForGame () = 
    async {
        let gameRunning = 
            IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
            |> Array.exists (fun x -> x.Port = 2999)

        if gameRunning then
            printfn "Game started!"
            return ()
        else 
            printfn "Game not started, waiting..."
            do! Async.Sleep 1000
            return! waitForGame ()
    }

let processEvents (events: EventJson.Root) =
    async {
        return printfn "Events: %A" events
    }

let rec programLoop () =
    async {
        do! waitForGame ()
        let! allInfo = getAllStats ()
        printWelcome allInfo
        do! loop allInfo.ActivePlayer.SummonerName processEvents
        return! programLoop ()
    }

[<EntryPoint>]
let main argv =
    programLoop () |> Async.RunSynchronously
    0
