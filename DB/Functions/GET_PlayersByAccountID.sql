CREATE OR REPLACE FUNCTION GET_PlayerByAccountID(_accountID INTEGER)
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
        AccountID = _accountID
$$ LANGUAGE SQL;
