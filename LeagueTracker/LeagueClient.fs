module LeagueClient

open RestSharp
open FSharp.Data
open System.Net.Security

let client =
    let c = RestClient("https://127.0.0.1:2999")
    c.RemoteCertificateValidationCallback <- RemoteCertificateValidationCallback(fun _ _ _ _ -> true)
    c

type ScoreJson = JsonProvider<"""
{
    "assists": 0,
    "creepScore": 0,
    "deaths": 0,
    "kills": 0,
    "wardScore": 0.0
}
""">
type Score = ScoreJson.Root

type AllJson = JsonProvider<"https://static.developer.riotgames.com/docs/lol/liveclientdata_sample.json">
type AllData = AllJson.Root

type EventJson = JsonProvider<"https://static.developer.riotgames.com/docs/lol/liveclientdata_events.json">
type Event = EventJson.Event

let getAllStats () : Async<AllData> =
    async {
        let req = RestRequest "liveclientdata/allgamedata"
        let! res = Async.AwaitTask <| client.ExecuteAsync req
        let content = res.Content
        return AllJson.Parse content
    }

let getPlayerName () : Async<string> =
    async {
        let req = RestRequest "liveclientdata/activeplayername"
        let! res = Async.AwaitTask <| client.ExecuteAsync<string> req
        return res.Data
    }

let getPlayerScore player : Async<Score> =
    async {
        let req = RestRequest("liveclientdata/playerscores").AddQueryParameter("summonerName", player)
        let! res = Async.AwaitTask <| client.ExecuteAsync req
        return ScoreJson.Parse res.Content
    }

let getEvents () : Async<Event list option> =
    async {
        try 
            let req = RestRequest "liveclientdata/eventdata"
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return
                (EventJson.Parse res.Content).Events
                |> List.ofArray
                |> Some
        with
        | _ -> return None
    }
