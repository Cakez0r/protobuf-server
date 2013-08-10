CREATE TABLE AbilityBehaviour
(
    AbilityBehaviourID SERIAL CONSTRAINT PK_AbilityBehaviour PRIMARY KEY,
    AbilityID INTEGER NOT NULL CONSTRAINT FK_AbilityBehaviour_Ability REFERENCES Ability,
    ExecutionOrder INTEGER NOT NULL,
    BehaviourType TEXT NOT NULL
);