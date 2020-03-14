module Events

open LeagueClient

type EventData = {
    EventID: int
    EventName: string
    EventTime: decimal
}

type GameStart = {
    Data: EventData
}

type MinionsSpawning = {
    Data: EventData
}

type FirstBrick = {
    Data: EventData
    KillerName: string
}

type TurretKilled = {
    Data: EventData
    TurretKilled: string
    KillerName: string
    Assisters: string seq
}

type InhibKilled = {
    Data: EventData
    InhibKilled: string
    KillerName: string
    Assisters: string seq
}

type DragonType =
    | Earth
    | Fire
    | Water
    | Air
    | Elder

type DragonKill = {
    Data: EventData
    DragonType: DragonType
    Stolen: bool
    KillerName: string
    Assisters: string seq
}

type HeraldKill = {
    Data: EventData
    HeraldKillerName: string
    Stolen: bool
    Assisters: string seq
}

type BaronKill = {
    Data: EventData
    BaronKillerName: string
    Stolen: bool
    Assisters: string seq
}

type ChampionKill = {
    Data: EventData
    VictimName: string
    KillerName: string
    Assisters: string seq
}

type Multikill = {
    Data: EventData
    KillerName: string
    KillStreak: int
}

type Team =
    | Order
    | Chaos

type Ace = {
    Data: EventData
    Acer: string
    AcingTeam: Team
}

type Event =
    | GameStart of GameStart
    | MinionsSpawning of MinionsSpawning
    | FirstBrick of FirstBrick
    | TurretKilled of TurretKilled
    | InhibKilled of InhibKilled
    | DragonKill of DragonKill
    | HeraldKill of HeraldKill
    | BaronKill of BaronKill
    | ChampionKill of ChampionKill
    | Multikill of Multikill
    | Ace of Ace

let parseEvent (e: EventJson.Event) : Event =
    let data = { EventID = e.EventId; EventName = e.EventName; EventTime = e.EventTime }

    match e.EventName with
    | "GameStart" ->
        GameStart {
            Data = data
        }
    | "MinionsSpawning" ->
        MinionsSpawning {
            Data = data
        }
    | "FirstBrick" ->
        FirstBrick {
            Data = data
            KillerName = e.KillerName.Value
        }
    | "TurretKilled" ->
        TurretKilled {
            Data = data
            TurretKilled = e.TurretKilled.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "InhibKilled" ->
        InhibKilled {
            Data = data
            InhibKilled = e.InhibKilled.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "DragonKill" ->
        DragonKill {
            Data = data
            DragonType =
                match e.DragonType.Value with
                | "Earth" -> Earth
                | "Water" -> Water
                | "Fire" -> Fire
                | "Air" -> Air
                | "Elder" -> Elder
                | _ -> failwithf "Unknown dragon type %s" e.DragonType.Value
            Stolen = e.Stolen.Value
            KillerName = e.KillerName.Value
            Assisters = e.Assisters
        }
    | "HeraldKill" ->
        HeraldKill {
            Data = data
            HeraldKillerName = e.KillerName.Value
            Stolen = e.Stolen.Value
            Assisters = e.Assisters
        }
    | "BaronKill" ->
        BaronKill {
            Data = data
            BaronKillerName = e.KillerName.Value
            Stolen = e.Stolen.Value
            Assisters = e.Assisters
        }
    | "ChampionKill" ->
        ChampionKill {
            Data = data
            KillerName = e.KillerName.Value
            VictimName = e.VictimName.Value
            Assisters = e.Assisters
        }
    | "Multikill" ->
        Multikill {
            Data = data
            KillerName = e.KillerName.Value
            KillStreak = e.KillStreak.Value
        }
    | "Ace" ->
        Ace {
            Data = data
            Acer = e.Acer.Value
            AcingTeam =
                match e.AcingTeam.Value with
                | "ORDER" -> Order
                | "CHAOS" -> Chaos
                | _ -> failwithf "Unknown acing team %s" e.AcingTeam.Value
        }
    | unknown -> failwithf "Unknown event type %s" unknown

