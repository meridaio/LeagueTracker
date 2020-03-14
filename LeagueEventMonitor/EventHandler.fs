module EventHandler

open Events

type EventContext = {
    SummonerName: string
}

[<AbstractClass>]
type EventHandler (ctx: EventContext) =
    member __.SummonerName = ctx.SummonerName

    abstract member on : Event -> bool
    abstract member onGameStart : GameStart -> unit
    abstract member onMinionsSpawning : MinionsSpawning -> unit
    abstract member onFirstBrick : FirstBrick -> unit
    abstract member onTurretKilled : TurretKilled -> unit
    abstract member onInhibKilled : InhibKilled -> unit
    abstract member onDragonKill : DragonKill -> unit
    abstract member onHeraldKill : HeraldKill -> unit
    abstract member onBaronKill : BaronKill -> unit
    abstract member onChampionKill : ChampionKill -> unit
    abstract member onMultikill : Multikill -> unit
    abstract member onAce : Ace -> unit

    default __.on _ = false
    default __.onGameStart _ = ()
    default __.onMinionsSpawning _ = ()
    default __.onFirstBrick _ = ()
    default __.onTurretKilled _ = ()
    default __.onInhibKilled _ = ()
    default __.onDragonKill _ = ()
    default __.onHeraldKill _ = ()
    default __.onBaronKill _ = ()
    default __.onChampionKill _ = ()
    default __.onMultikill _ = ()
    default __.onAce _ = ()
