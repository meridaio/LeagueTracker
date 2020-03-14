module internal LeagueClient

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

[<Literal>]
let allSource = __SOURCE_DIRECTORY__ + "/Data/allgamedata.json"
type AllJson = JsonProvider<allSource>
type AllData = AllJson.Root

[<Literal>]
let eventSource = __SOURCE_DIRECTORY__ + "/Data/events.json"
type EventJson = JsonProvider<eventSource>

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

let getNextEvents (id: int) : Async<EventJson.Event list option> =
    async {
        try
            let req = RestRequest("liveclientdata/eventdata").AddQueryParameter("eventID", id.ToString())
            let! res = Async.AwaitTask <| client.ExecuteAsync req
            return
                (EventJson.Parse res.Content).Events
                |> List.ofArray
                |> Some
        with
        | _ -> return None
    }

let getEvents () : Async<EventJson.Event list option> =
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
