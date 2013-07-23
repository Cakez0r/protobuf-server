CREATE OR REPLACE FUNCTION GET_NPCSpawns()
RETURNS SETOF NPCSpawn
AS $$
    SELECT
        NPCSpawnId,
        NPCID,
        Position,
        Rotation,
        MapNumber,
        Frequency,
        Flags
    FROM
        NPCSpawn
$$ LANGUAGE SQL;
