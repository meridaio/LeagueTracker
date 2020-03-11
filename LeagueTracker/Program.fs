open System

open EventProcessor
open LeagueClient
open System.Net.NetworkInformation

let printWelcome (allInfo: AllData) =
    printfn "Hello, %s! Welcome to League Tracker!" allInfo.ActivePlayer.SummonerName

let loop (callback: Event list -> Async<unit>) : Async<unit> =
    let rec loop' (prevState: Event list option) =
        async {
            let! events = getEvents ()
            let processEvents e =
                async {
                    do! callback e
                    do! Async.Sleep 500
                    return! loop' events
                }

            do! match events, prevState with
                | Some e, Some p -> List.except p e |> processEvents
                | Some e, None   -> e |> processEvents
                | _              -> async { () }
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

let rec programLoop () : Async<unit> =
    async {
        do! waitForGame ()
        do! waitForApi ()
        let currentTime = DateTime.Now
        let! allInfo = getAllStats ()
        let startTime = currentTime.AddSeconds(-(float allInfo.GameData.GameTime))
        printfn "Start time: %A" startTime
        printWelcome allInfo
        do! processEvents allInfo.ActivePlayer.SummonerName |> loop
        return! programLoop ()
    }

[<EntryPoint>]
let main argv =
    programLoop () |> Async.RunSynchronously
    0
