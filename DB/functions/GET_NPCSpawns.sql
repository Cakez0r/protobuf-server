CREATE OR REPLACE FUNCTION GET_NPCSpawns()
RETURNS SETOF NPCSpawnModel
AS $$
    SELECT
        NPCSpawnId,
        NPCID,
        Position[0] AS X,
        Position[1] AS Y,
        Rotation,
        MapNumber,
        Frequency,
        Flags
    FROM
        NPCSpawn
$$ LANGUAGE SQL;
