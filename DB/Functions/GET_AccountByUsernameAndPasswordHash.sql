CREATE OR REPLACE FUNCTION GET_AccountByAccountUsernameAndPasswordHash(_username TEXT, _passwordHash TEXT)
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
        Username = _username AND
        PasswordHash = _passwordHash
$$ LANGUAGE SQL;
