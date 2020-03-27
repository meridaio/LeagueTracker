module ConsoleHandler

open System.Drawing
open EventHandler
open MagicHome
open LeagueEventMonitor.Client.Events
open LeagueEventMonitor.Client.LeagueClient

type ConsoleHandler (ctx: EventContext, light: Light) =
    inherit EventHandler (ctx)
    
    override self.on e =
        match e with
        | ChampionKill c ->
            match c.VictimName, c.KillerName, c.Assisters with
            | v, _, _ when v = self.SummonerName ->
                printfn "You died!! lol!!"
                async {
                    do! light.SetColorAsync(Color.Red) |> Async.AwaitTask
                    let! playerList = getPlayerList ()
                    match playerList with
                    | Some p -> 
                        let player = p |> Array.find (fun x -> x.SummonerName = self.SummonerName)
                        do! Async.Sleep <| int (player.RespawnTimer * 1000M)
                        do! light.SetColorAsync(Color.Green) |> Async.AwaitTask
                    | None ->
                        printfn "Error: unable to get player respawn timer"
                } |> Async.Start
                true
            | v, k, a when k = self.SummonerName && Seq.isEmpty a ->
                printfn "You killed %s all by yourself!" v
                true
            | v, k, a when k = self.SummonerName ->
                printfn "You killed %s with some help from %s" v (String.concat ", " a)
                true
            | v, k, a when a |> Seq.exists (fun x -> x = self.SummonerName) ->
                printfn "You assisted %s killing %s" k v
                true
            | _ -> false
        | GameStart _ ->
            printfn "GLHF!"
            true
        | _ ->
            printfn "Something else happened! (%A)" e
            false
