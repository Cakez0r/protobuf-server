using System;
using System.Collections.Generic;

namespace Server.NPC
{
    public interface INPCBehaviour
    {
        void Initialise(IReadOnlyDictionary<string, string> vars);
        void Update(TimeSpan dt, NPCInstance npc);
    }
}
