CREATE TABLE AbilityBehaviourVar
(
    AbilityBehaviourVarID SERIAL CONSTRAINT PK_AbilityBehaviourVar PRIMARY KEY,
    AbilityBehaviourID INTEGER NOT NULL CONSTRAINT FK_AbilityBehaviourVar_AbilityBehaviour REFERENCES AbilityBehaviour,
    Key TEXT NOT NULL,
    Value TEXT NOT NULL
);