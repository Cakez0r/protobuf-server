CREATE OR REPLACE FUNCTION UPD_Account(_accountID INTEGER, _passwordHash TEXT, _email TEXT, _lastLoginDate TIMESTAMP) 
RETURNS Account 
AS $$
    UPDATE
        Account
    SET
        PasswordHash = _passwordHash,
        Email = _email,
        LastLoginDate = _lastLoginDate
    WHERE
        AccountID = _accountID
    RETURNING
        AccountID,
        Username,
        PasswordHash,
        Email,
        DateCreated,
        LastLoginDate;
$$ LANGUAGE SQL;
