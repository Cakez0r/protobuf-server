CREATE OR REPLACE FUNCTION GET_NPCBehaviours()
RETURNS SETOF NPCBehaviour
AS $$
    SELECT
        NPCBehaviourID,
        NPCID,
        NPCBehaviourType,
        ExecutionOrder
    FROM
        NPCBehaviour
$$ LANGUAGE SQL;
