module ConsoleHandler

open System.Drawing
open EventHandler
open LEDController.Controller
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
                        controller.enqueueEvent <| Die (int (player.RespawnTimer * 1000M))
                    | None ->
                        printfn "Error: unable to get player respawn timer"
                } |> Async.Start
                true
            | v, k, a when k = self.SummonerName && Seq.isEmpty a ->
                controller.enqueueEvent <| Kill
                printfn "You killed %s all by yourself!" v
                true
            | v, k, a when k = self.SummonerName ->
                controller.enqueueEvent <| Kill
                printfn "You killed %s with some help from %s" v (String.concat ", " a)
                true
            | v, k, a when a |> Seq.contains self.SummonerName ->
                printfn "You assisted %s killing %s" k v
                true
            | _ -> false
        | GameStart _ ->
            printfn "GLHF!"
            true
        | BaronKill b ->
            controller.enqueueEvent <| Baron b.Stolen
            true
        | DragonKill d ->
            controller.enqueueEvent <|
                match d.DragonType with
                | Earth -> EarthDrag
                | Air -> AirDrag
                | Water -> WaterDrag
                | Fire -> FireDrag
                | Elder -> ElderDrag
            true
        | _ ->
            printfn "Something else happened! (%A)" e
            false
