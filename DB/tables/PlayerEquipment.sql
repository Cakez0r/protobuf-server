CREATE TABLE PlayerEquipment
(
    PlayerEquipmentID SERIAL CONSTRAINT PK_PlayerEquipment PRIMARY KEY,
    PlayerID INTEGER NOT NULL CONSTRAINT FK_PlayerEquipment_Player REFERENCES Player,
    ItemInstanceID BIGINT NOT NULL CONSTRAINT FK_PlayerEquipment_ItemInstance REFERENCES ItemInstance,
    SlotNumber INTEGER NOT NULL
);