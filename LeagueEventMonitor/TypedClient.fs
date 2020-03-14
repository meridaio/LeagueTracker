module TypedClient

open Events
open LeagueClient

let getEvents () : Async<Event list option> =
    async {
        let! events = getEvents ()
        return events |> Option.map (List.map parseEvent)
    }
