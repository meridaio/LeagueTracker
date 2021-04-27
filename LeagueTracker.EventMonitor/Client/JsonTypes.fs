module internal LeagueTracker.EventMonitor.Client.JsonTypes

open FSharp.Data

type ScoreJson = JsonProvider<"""
{
    "assists": 0,
    "creepScore": 0,
    "deaths": 0,
    "kills": 0,
    "wardScore": 0.1
}
""">

[<Literal>]
let allSource = __SOURCE_DIRECTORY__ + "/../Data/allgamedata.json"
type AllJson = JsonProvider<allSource>

[<Literal>]
let eventSource = __SOURCE_DIRECTORY__ + "/../Data/events.json"
type EventJson = JsonProvider<eventSource>

[<Literal>]
let playerlistSource = __SOURCE_DIRECTORY__ + "/../Data/playerlist.json"
type PlayerListJson = JsonProvider<playerlistSource>
