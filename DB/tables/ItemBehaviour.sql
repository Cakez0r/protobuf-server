CREATE TABLE ItemBehaviour
(
    ItemBehaviourID SERIAL CONSTRAINT PK_ItemBehaviour PRIMARY KEY,
    ItemID INTEGER NOT NULL CONSTRAINT FK_ItemBehaviour_Item REFERENCES Item,
    ItemBehaviourType TEXT NOT NULL
);