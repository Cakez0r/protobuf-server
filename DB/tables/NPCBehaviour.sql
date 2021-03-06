CREATE TABLE NPCBehaviour
(
    NPCBehaviourID SERIAL CONSTRAINT PK_NPCBehaviour PRIMARY KEY,
    NPCID INTEGER NOT NULL CONSTRAINT FK_NPCBehaviour_NPC REFERENCES NPC,
    ExecutionOrder INTEGER NOT NULL,
    NPCBehaviourType TEXT NOT NULL
);