CREATE OR REPLACE FUNCTION GET_AccountByAccountUsername(_username TEXT)
RETURNS Account
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
        Username = _username
$$ LANGUAGE SQL;
