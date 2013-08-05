CREATE OR REPLACE FUNCTION GET_NPCBehaviourVars()
RETURNS SETOF NPCBehaviourVar
AS $$
    SELECT
        NPCBehaviourVarID,
        NPCBehaviourID,
        Key,
        Value
    FROM
        NPCBehaviourVar
$$ LANGUAGE SQL;
