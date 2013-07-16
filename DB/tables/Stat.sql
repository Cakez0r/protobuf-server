CREATE TABLE Stat
(
    StatID SERIAL CONSTRAINT PK_Stat PRIMARY KEY,
    StatFriendlyName TEXT NOT NULL
);

CREATE UNIQUE INDEX UQ_Stat_StatFriendlyName ON Stat (LOWER(StatFriendlyName));