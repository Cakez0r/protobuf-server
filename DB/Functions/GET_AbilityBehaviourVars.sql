CREATE OR REPLACE FUNCTION GET_AbilityBehaviourVars()
RETURNS SETOF AbilityBehaviourVar
AS $$
    SELECT
        AbilityBehaviourVarID,
        AbilityBehaviourID,
        Key,
        Value
    FROM
        AbilityBehaviourVar;
$$ LANGUAGE SQL;