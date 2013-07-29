using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.NPCs
{
    public class NPCSpawnModel
    {
        public int NPCSpawnID { get; set; }
        public int NPCID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public float Rotation { get; set; }
        public int MapNumber { get; set; }
        public TimeSpan Frequency { get; set; }
        public int Flags { get; set; }
    }
}
