CREATE TABLE AbilityRequirement
(
    AbilityRequirementID SERIAL CONSTRAINT PK_AbilityRequirement PRIMARY KEY,
    AbilityID INTEGER NOT NULL CONSTRAINT FK_AbilityRequirement_Ability REFERENCES Ability,
    StatID INTEGER NOT NULL CONSTRAINT FK_AbilityRequirement_Stat REFERENCES Stat,
    StatValue REAL NOT NULL
);