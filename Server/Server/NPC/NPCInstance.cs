using Data.NPCs;
using Protocol;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NPC
{
    public class NPCInstance
    {
        public NPCModel NPCModel { get; set; }

        public NPCStateUpdate LatestStateUpdate { get; set; }
    }
}
