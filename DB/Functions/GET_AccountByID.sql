﻿CREATE OR REPLACE FUNCTION GET_AccountByID(_accountID INTEGER)
RETURNS SETOF Account
AS $$
    SELECT
        AccountID,
        Username,
        PasswordHash,
        Email,
        DateCreated,
        LastLoginDate
    FROM
        Account
    WHERE
        AccountID = _accountID
$$ LANGUAGE SQL;
