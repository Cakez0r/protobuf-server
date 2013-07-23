CREATE OR REPLACE FUNCTION GET_AccountByUsernameAndPasswordHash(_username TEXT, _passwordHash TEXT)
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
        LOWER(Username) = LOWER(_username) AND
        PasswordHash = _passwordHash
$$ LANGUAGE SQL;
