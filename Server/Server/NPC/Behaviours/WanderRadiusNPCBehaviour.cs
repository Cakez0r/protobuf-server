using Data.NPCs;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NPC.Behaviours
{
    public class WanderRadiusNPCBehaviour : INPCBehaviour
    {
        private const float WALK_SPEED = 7;

        private float m_radius;

        private Vector2? m_target;

        private Random m_rng = new Random();

        public void Initialise(IReadOnlyDictionary<string, string> vars)
        {
            m_radius = float.Parse(vars["Radius"]);
        }

        public void Update(TimeSpan dt, NPCInstance npc)
        {
            if (m_target == null)
            {
                m_target = GetRandomTarget(new Vector2((float)npc.NPCSpawnModel.X, (float)npc.NPCSpawnModel.Y), m_radius);
            }

            if (Vector2.DistanceSquared(m_target.Value, npc.Position) < 4)
            {
                m_target = GetRandomTarget(new Vector2((float)npc.NPCSpawnModel.X, (float)npc.NPCSpawnModel.Y), m_radius);
            }

            Vector2 delta = m_target.Value - npc.Position;
            delta.Normalize();
            delta *= WALK_SPEED * (float)dt.TotalSeconds;

            npc.Position += delta;
        }

        private Vector2 GetRandomTarget(Vector2 origin, float radius)
        {
            double theta = m_rng.NextDouble() * Math.PI * 2;
            double r = m_rng.NextDouble() * radius;
            return origin + new Vector2((float)(Math.Sin(theta) * r), (float)(Math.Cos(theta) * r));
        }
    }
}
