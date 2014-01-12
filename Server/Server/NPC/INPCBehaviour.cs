using System;
using System.Collections.Generic;

namespace Server.NPC
{
    public interface INPCBehaviour
    {
        bool Enabled { get; set; }

        void Initialise(IReadOnlyDictionary<string, string> vars);
        void Update(TimeSpan dt, NPCInstance npc);
    }
}
