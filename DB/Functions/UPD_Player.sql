CREATE OR REPLACE FUNCTION UPD_Player(_playerID INTEGER, _accountID INTEGER, _name TEXT, _health REAL, _power REAL, _money BIGINT, _map INTEGER, _position POINT, _rotation REAL)
RETURNS VOID
AS $$
    UPDATE
        Player
    SET
        AccountID = _accountID,
        Name = _name,
        Health = _health,
        Power = _power,
        Money = _money,
        Map = _map,
        Position = _position,
        Rotation = _rotation
    WHERE
        PlayerID = _playerID
$$ LANGUAGE SQL;
