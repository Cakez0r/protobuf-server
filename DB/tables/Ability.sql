CREATE TABLE Ability
(
    AbilityID SERIAL CONSTRAINT PK_Ability PRIMARY KEY,
    InternalName TEXT NOT NULL,
    AbilityType INTEGER NOT NULL,
    TargetType INTEGER NOT NULL,
    CastType INTEGER NOT NULL,
    TargetHealthDelta INTEGER NOT NULL,
    TargetPowerDelta INTEGER NOT NULL,
    SourceHealthDelta INTEGER NOT NULL,
    SourcePowerDelta INTEGER NOT NULL,
    CastTimeMS INTEGER NOT NULL,
    Range INTEGER NOT NULL,
    ClassAffinity INTEGER NOT NULL,
    ThreatModifier REAL NOT NULL DEFAULT 1
);
