CREATE OR REPLACE FUNCTION GET_PlayerStatsByPlayerID(_playerID INTEGER)
RETURNS SETOF PlayerStat
AS $$
    SELECT
        PlayerStatID,
        PlayerID,
        StatID,
        StatValue
    FROM
        PlayerStat
    WHERE
        PlayerID = _playerID
$$ LANGUAGE SQL;
