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

type AllJson = JsonProvider<"https://static.developer.riotgames.com/docs/lol/liveclientdata_sample.json">

type EventJson = JsonProvider<"https://static.developer.riotgames.com/docs/lol/liveclientdata_events.json">

let getAllStats () =
    async {
        let req = RestRequest "liveclientdata/allgamedata"
        let! res = Async.AwaitTask <| client.ExecuteAsync req
        let content = res.Content
        return AllJson.Parse content
    }

let getPlayerName () =
    async {
        let req = RestRequest "liveclientdata/activeplayername"
        let! res = Async.AwaitTask <| client.ExecuteAsync<string> req
        return res.Data
    }

let getPlayerScore player =
    async {
        let req = RestRequest("liveclientdata/playerscores").AddQueryParameter("summonerName", player)
        let! res = Async.AwaitTask <| client.ExecuteAsync req
        return ScoreJson.Parse res.Content
    }

let getEvents () =
    async {
        try 
            let req = RestRequest "liveclientdata/eventdata"
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return Some <| EventJson.Parse res.Content
        with
        | _ -> return None
    }
