module LeagueEventMonitor.Client.LeagueClient

open System.Net.Security
open LeagueEventMonitor.Client.Events
open LeagueEventMonitor.Client.JsonTypes
open RestSharp
open Score

let client =
    let c = RestClient("https://127.0.0.1:2999")
    c.RemoteCertificateValidationCallback <- RemoteCertificateValidationCallback(fun _ _ _ _ -> true)
    c

let internal parseScore (s: ScoreJson.Root) : Score =
    {
        Assists = s.Assists
        CreepScore = s.CreepScore
        Deaths = s.Deaths
        Kills = s.Kills
        WardScore = s.WardScore
    }

let internal parseEvent (e: EventJson.Event) : Event =
    let data = { EventID = e.EventId; EventName = e.EventName; EventTime = e.EventTime }

    match e.EventName with
    | "GameStart" ->
        GameStart {
            Data = data
        }
    | "MinionsSpawning" ->
        MinionsSpawning {
            Data = data
        }
    | "FirstBrick" ->
        FirstBrick {
            Data = data
            KillerName = e.KillerName.Value
        }
    | "TurretKilled" ->
        TurretKilled {
            Data = data
            TurretKilled = e.TurretKilled.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "InhibKilled" ->
        InhibKilled {
            Data = data
            InhibKilled = e.InhibKilled.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "DragonKill" ->
        DragonKill {
            Data = data
            DragonType =
                match e.DragonType.Value with
                | "Earth" -> Earth
                | "Water" -> Water
                | "Fire" -> Fire
                | "Air" -> Air
                | "Elder" -> Elder
                | _ -> failwithf "Unknown dragon type %s" e.DragonType.Value
            Stolen = e.Stolen.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "HeraldKill" ->
        HeraldKill {
            Data = data
            HeraldKillerName = e.KillerName.Value
            Stolen = e.Stolen.Value
            Assisters = e.Assisters
        }
    | "BaronKill" ->
        BaronKill {
            Data = data
            BaronKillerName = e.KillerName.Value
            Stolen = e.Stolen.Value
            Assisters = e.Assisters
        }
    | "ChampionKill" ->
        ChampionKill {
            Data = data
            KillerName = e.KillerName.Value
            VictimName = e.VictimName.Value
            Assisters = e.Assisters
        }
    | "Multikill" ->
        Multikill {
            Data = data
            KillerName = e.KillerName.Value
            KillStreak = e.KillStreak.Value
        }
    | "Ace" ->
        Ace {
            Data = data
            Acer = e.Acer.Value
            AcingTeam =
                match e.AcingTeam.Value with
                | "ORDER" -> Order
                | "CHAOS" -> Chaos
                | _ -> failwithf "Unknown acing team %s" e.AcingTeam.Value
        }
    | unknown -> failwithf "Unknown event type %s" unknown

type AllData = AllJson.Root

/// Gets all live data for the current game -- should be called sparingly
let getAllStats () : Async<AllData option> =
    async {
        try
            let req = RestRequest "liveclientdata/allgamedata"
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            let content = res.Content
            return AllJson.Parse content |> Some
        with
        | _ -> return None
    }

/// Gets the current active summoner name
let getPlayerName () : Async<string option> =
    async {
        try
            let req = RestRequest "liveclientdata/activeplayername"
            let! res = Async.AwaitTask <| client.ExecuteAsync<string> req
            return res.Data |> Some
        with
        | _ -> return None
    }

/// Given a summoner name, get their current score
let getPlayerScore player : Async<Score option> =
    async {
        try
            let req = RestRequest("liveclientdata/playerscores").AddQueryParameter("summonerName", player)
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return ScoreJson.Parse res.Content
            |> parseScore
            |> Some
        with
        | _ -> return None
    }

/// Gets all events with ID greater than or equal to the provdied ID
let getNextEvents (id: int) : Async<Event list option> =
    async {
        try
            let req = RestRequest("liveclientdata/eventdata").AddQueryParameter("eventID", id.ToString())
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return
                (EventJson.Parse res.Content).Events
                |> List.ofArray
                |> List.map parseEvent
                |> Some
        with
        | _ -> return None
    }

/// Gets all events that have happened in game so far
let getEvents () : Async<Event list option> =
    async {
        try 
            let req = RestRequest "liveclientdata/eventdata"
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return
                (EventJson.Parse res.Content).Events
                |> List.ofArray
                |> List.map parseEvent
                |> Some
        with
        | _ -> return None
    }

let getPlayerList () =
    async {
        try
            let req = RestRequest "liveclientdata/playerlist"
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return
                (PlayerListJson.Parse res.Content)
                |> Some
        with
        | _ -> return None
    }

