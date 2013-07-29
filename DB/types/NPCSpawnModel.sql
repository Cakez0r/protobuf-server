﻿CREATE TYPE NPCSpawnModel AS
(
    NPCSpawnID INTEGER,
    NPCID INTEGER,
    X DOUBLE PRECISION,
    Y DOUBLE PRECISION,
    Rotation REAL,
    MapNumber INTEGER,
    Frequency INTERVAL,
    Flags INTEGER
);
