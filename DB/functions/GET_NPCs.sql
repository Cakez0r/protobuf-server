CREATE OR REPLACE FUNCTION GET_NPCs()
RETURNS SETOF NPC
AS $$
    SELECT
        NPCID,
        Name,
        Model,
        Scale
    FROM
        NPC;
$$ LANGUAGE SQL;
