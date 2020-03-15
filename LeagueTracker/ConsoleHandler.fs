module ConsoleHandler

open EventHandler
open LeagueEventMonitor.Client.Events

type ConsoleHandler (ctx: EventContext) =
    inherit EventHandler (ctx)
    
    override self.on e =
        match e with
        | ChampionKill c ->
            match c.VictimName, c.KillerName, c.Assisters with
            | v, _, _ when v = self.SummonerName ->
                printfn "You died!! lol!!"
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
