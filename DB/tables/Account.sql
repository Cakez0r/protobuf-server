CREATE TABLE Account
(
    AccountID SERIAL CONSTRAINT PK_Account PRIMARY KEY,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Email TEXT NOT NULL,
    DateCreated TIMESTAMP NOT NULL,
    LastLoginDate TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX UQ_Account_Username ON Account (LOWER(Username));
CREATE UNIQUE INDEX UQ_Account_Email ON Account (LOWER(Email));