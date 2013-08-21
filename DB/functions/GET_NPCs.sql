CREATE OR REPLACE FUNCTION GET_NPCs()
RETURNS SETOF NPC
AS $$
    SELECT
        NPCID,
        Name,
        Model,
        Scale,
        InternalName
    FROM
        NPC;
$$ LANGUAGE SQL;
