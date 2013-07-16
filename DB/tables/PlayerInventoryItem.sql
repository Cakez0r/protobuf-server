CREATE TABLE PlayerInventoryItem
(
    PlayerInventoryID SERIAL CONSTRAINT PK_PlayerInventoryItem PRIMARY KEY,
    PlayerID INTEGER NOT NULL CONSTRAINT FK_PlayerInventoryItem_Player REFERENCES Player,
    ItemInstanceID BIGINT NOT NULL CONSTRAINT FK_PlayerInventoryItem_ItemInstance REFERENCES ItemInstance,
    SlotNumber INTEGER NOT NULL,
    StackSize INTEGER NOT NULL
);