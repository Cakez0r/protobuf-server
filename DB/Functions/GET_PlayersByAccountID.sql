CREATE OR REPLACE FUNCTION GET_PlayersByAccountID(_accountID INTEGER)
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
        AccountID = _accountID
$$ LANGUAGE SQL;
