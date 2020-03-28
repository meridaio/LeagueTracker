module ConsoleHandler

open System.Drawing
open EventHandler
open LEDManager
open LeagueEventMonitor.Client.Events
open LeagueEventMonitor.Client.LeagueClient

type ConsoleHandler (ctx: EventContext, controller: LEDController) =
    inherit EventHandler (ctx)
    
    override self.on e =
        match e with
        | ChampionKill c ->
            match c.VictimName, c.KillerName, c.Assisters with
            | v, _, _ when v = self.SummonerName ->
                printfn "You died!! lol!!"
                async {
                    let! playerList = getPlayerList ()
                    match playerList with
                    | Some p -> 
                        let player = p |> Array.find (fun x -> x.SummonerName = self.SummonerName)
                        do! controller.death <| int (player.RespawnTimer * 1000M)
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
        | BaronKill b ->
            async {
                if b.Stolen then
                    do! controller.baronSteal ()
                else
                    do! controller.baronKill ()

            } |> Async.Start
            true
        | DragonKill d ->
            async {
                if d.Stolen then
                    do! controller.dragonSteal ()
                else
                    do! controller.dragonKill d.DragonType
            } |> Async.Start
            true
        | _ ->
            printfn "Something else happened! (%A)" e
            false
