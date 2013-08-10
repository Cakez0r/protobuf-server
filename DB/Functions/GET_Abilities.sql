CREATE OR REPLACE FUNCTION GET_Abilities()
RETURNS SETOF Ability
AS $$
    SELECT
        AbilityID,
        FriendlyName
    FROM
        Ability;
$$ LANGUAGE SQL;