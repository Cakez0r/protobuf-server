CREATE OR REPLACE FUNCTION GET_AccountByUsername(_username TEXT)
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
        LOWER(Username) = LOWER(_username)
$$ LANGUAGE SQL;
