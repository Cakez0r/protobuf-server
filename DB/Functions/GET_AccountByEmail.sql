CREATE OR REPLACE FUNCTION GET_AccountByEmail(_email TEXT)
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
        LOWER(Email) = LOWER(_email)
$$ LANGUAGE SQL;
