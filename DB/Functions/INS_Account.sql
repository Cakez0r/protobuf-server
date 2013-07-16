CREATE OR REPLACE FUNCTION INS_Account(_username TEXT, _passwordHash TEXT, _email TEXT) 
RETURNS Account 
AS $$
    INSERT INTO
        Account
        (
            Username,
            PasswordHash,
            Email,
            DateCreated,
            LastLoginDate
        )
    VALUES
        (
            _username,
            _passwordHash,
            _email,
            current_timestamp,
            current_timestamp
        )
    RETURNING
        AccountID,
        Username,
        PasswordHash,
        Email,
        DateCreated,
        LastLoginDate
$$ LANGUAGE SQL;
