module LeagueEventMonitor.Client.Events

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

