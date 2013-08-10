CREATE OR REPLACE FUNCTION GET_AbilityBehaviours()
RETURNS SETOF AbilityBehaviour
AS $$
    SELECT
        AbilityBehaviourID,
        AbilityID,
        BehaviourType,
        ExecutionOrder
    FROM
        AbilityBehaviour;
$$ LANGUAGE SQL;