CREATE TABLE Item
(
    ItemID SERIAL CONSTRAINT PK_Item PRIMARY KEY,
    Name TEXT NOT NULL,
    Model TEXT NOT NULL,
    Scale REAL NOT NULL,
    Quality SMALLINT NOT NULL,
    MonetaryValue INTEGER NOT NULL,
    MaxStackSize INTEGER NOT NULL,
    Flags INTEGER NOT NULL
);