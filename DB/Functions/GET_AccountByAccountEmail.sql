CREATE OR REPLACE FUNCTION GET_AccountByAccountEmail(_email TEXT)
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
        Email = _email
$$ LANGUAGE SQL;
