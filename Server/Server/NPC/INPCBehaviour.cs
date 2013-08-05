using Data.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NPC
{
    public interface INPCBehaviour
    {
        void Initialise(IReadOnlyDictionary<string, string> vars);
        void Update(TimeSpan dt, NPCInstance npc);
    }
}
