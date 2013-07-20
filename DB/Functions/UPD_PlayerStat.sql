CREATE OR REPLACE FUNCTION UPD_PlayerStat(_playerStatID INTEGER, _playerID INTEGER, _statID INTEGER, _statValue REAL)
RETURNS VOID
AS $$
    UPDATE
        PlayerStat
    SET
        StatValue = _statValue
    WHERE
        PlayerStatID = _playerStatID AND
        StatID = _statID AND
        PlayerID = _playerID
$$ LANGUAGE SQL;
