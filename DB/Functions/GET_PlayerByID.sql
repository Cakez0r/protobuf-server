CREATE OR REPLACE FUNCTION GET_PlayerByID(_playerID INTEGER)
RETURNS SETOF Player
AS $$
    SELECT
        PlayerID,
        AccountID,
        Name,
        Health,
        Power,
        Money,
        Map,
        Position,
        Rotation
    FROM
        Player
    WHERE
        PlayerID = _playerID;
$$ LANGUAGE SQL;
