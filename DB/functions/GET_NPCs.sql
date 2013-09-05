CREATE OR REPLACE FUNCTION GET_NPCs()
RETURNS SETOF NPC
AS $$
    SELECT
        NPCID,
        Name,
        Scale,
        ModelID
    FROM
        NPC;
$$ LANGUAGE SQL;
