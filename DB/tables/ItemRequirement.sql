CREATE TABLE ItemRequirement
(
    ItemRequirementID SERIAL CONSTRAINT PK_ItemRequirement PRIMARY KEY,
    ItemID INTEGER NOT NULL CONSTRAINT FK_ItemRequirement_Item REFERENCES Item,
    StatID INTEGER NOT NULL CONSTRAINT FK_ItemRequirement_Stat REFERENCES Stat,
    StatValue REAL NOT NULL
);