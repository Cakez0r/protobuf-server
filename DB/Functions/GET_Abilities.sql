CREATE OR REPLACE FUNCTION GET_Abilities()
RETURNS SETOF Ability
AS $$
    SELECT
        AbilityID,
        FriendlyName,
        AbilityType,
        TargetType,
        CastType,
        TargetHealthDelta,
        TargetPowerDelta,
        SourceHealthDelta,
        SourcePowerDelta,
        CastTimeMS,
        Range,
        ClassAffinity,
        ThreatModifier
    FROM
        Ability;
$$ LANGUAGE SQL;
