﻿module EventProcessor

open LeagueClient

let processEvent (summonerName: string) (e: Event) : Async<unit> =
    async {
        match e.EventName with
        | "ChampionKill" ->
            match e.VictimName, e.KillerName, e.Assisters with
            | Some v, _, _ when v = summonerName ->
                printfn "You died!! lol!!"
            | Some v, Some k, [||] when k = summonerName ->
                printfn "You killed %s all by yourself!" v
            | Some v, Some k, a when k = summonerName ->
                printfn "You killed %s with some help from %s" v (String.concat ", " a)
            | Some v, Some k, a when a |> Array.exists (fun x -> x = summonerName) ->
                printfn "You assisted %s killing %s" k v
            | _ -> ()
        | "GameStart" ->
            printfn "GLHF!"
        | _ ->
            printfn "Something else happened! (%s)" e.EventName
    }

let processEvents (summonerName: string) (events: Event list) : Async<unit> =
    async {
        match events with
        | [] -> ()
        | _ ->
            printfn "New events: %A" events
            do! List.map <| processEvent summonerName <| events |> Async.Sequential |> Async.Ignore
    }
