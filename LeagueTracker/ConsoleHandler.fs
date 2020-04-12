module LeagueTracker.ConsoleHandler

open EventHandler
open LEDController.Controller
open LeagueEventMonitor.Client.Events
open LeagueEventMonitor.Client.LeagueClient

type ConsoleHandler (ctx: EventContext, controller: LEDController) =
    inherit EventHandler (ctx)

    override __.start () =
        controller.start ()

    override __.stop () =
        controller.stop ()
    
    override self.on e =
        match e with
        | ChampionKill c ->
            match c.VictimName, c.KillerName, c.Assisters with
            | v, _, _ when v = self.SummonerName ->
                printfn "You died!! lol!!"
                async {
                    let! playerList = getPlayerList ()
                    match playerList with
                    | Ok p -> 
                        let player = p |> Array.find (fun x -> x.SummonerName = self.SummonerName)
                        controller.enqueueEvent <| Die (int (player.RespawnTimer * 1000M))
                    | Error e ->
                        printfn "Error: unable to get player respawn timer: %A" e
                } |> Async.Start
                true
            | v, k, a when k = self.SummonerName && Seq.isEmpty a ->
                controller.enqueueEvent Kill
                printfn "You killed %s all by yourself!" v
                true
            | v, k, a when k = self.SummonerName ->
                controller.enqueueEvent Kill
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
        | GameEnd g ->
            printfn "Game ended: %A" g.Result
            true
        | _ ->
            printfn "Something else happened! (%A)" e
            false
