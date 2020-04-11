module LeagueTracker.Config

open System.IO
open Newtonsoft.Json

type Config () =
    member val ledIP = "" with get, set

let loadConfig () =
    if not (File.Exists("config.json")) then
        failwith "Error: no config.json found"
    else
        File.ReadAllText("config.json") |> JsonConvert.DeserializeObject<Config>
