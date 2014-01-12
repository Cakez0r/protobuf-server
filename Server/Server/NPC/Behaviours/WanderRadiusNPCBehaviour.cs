using System;
using System.Collections.Generic;

namespace Server.NPC.Behaviours
{
    public class WanderRadiusNPCBehaviour : INPCBehaviour
    {
        public bool Enabled { get; set; }

        private float m_walkSpeed;
        private float m_radius;

        public void Initialise(IReadOnlyDictionary<string, string> vars)
        {
            m_radius = float.Parse(vars["Radius"]);
            m_walkSpeed = float.Parse(vars["Speed"]);
        }

        public void Update(TimeSpan dt, NPCInstance npc)
        {
        }
    }
}
