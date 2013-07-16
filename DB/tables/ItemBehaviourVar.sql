CREATE TABLE ItemBehaviourVar
(
    ItemBehaviourVarID SERIAL CONSTRAINT PK_ItemBehaviourVar PRIMARY KEY,
    ItemBehaviourID INTEGER NOT NULL CONSTRAINT FK_ItemBehaviourVar_ItemBehaviour REFERENCES ItemBehaviour,
    Key TEXT NOT NULL,
    Value TEXT NOT NULL
);