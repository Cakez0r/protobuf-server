CREATE OR REPLACE FUNCTION GET_NPCBehaviours()
RETURNS SETOF NPCBehaviour
AS $$
    SELECT
        NPCBehaviourID,
        NPCID,
        NPCBehaviourType
    FROM
        NPCBehaviour
$$ LANGUAGE SQL;
