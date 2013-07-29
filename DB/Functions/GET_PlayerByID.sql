CREATE OR REPLACE FUNCTION GET_PlayerByID(_playerID INTEGER)
RETURNS SETOF PlayerModel
AS $$
    SELECT
        PlayerID,
        AccountID,
        Name,
        Health,
        Power,
        Money,
        Map,
        Position[0] AS X,
        Position[1] AS Y,
        Rotation
    FROM
        Player
    WHERE
        PlayerID = _playerID;
$$ LANGUAGE SQL;
