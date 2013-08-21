using Server.Utility;
using System;
using System.Collections.Generic;

namespace Server.NPC.Behaviours
{
    public class WanderRadiusNPCBehaviour : INPCBehaviour
    {
        private float m_walkSpeed;

        private float m_radius;

        private Vector2? m_target;

        private Random m_rng;

        public WanderRadiusNPCBehaviour()
        {
            m_rng = new Random(GetHashCode());
        }

        public void Initialise(IReadOnlyDictionary<string, string> vars)
        {
            m_radius = float.Parse(vars["Radius"]);
            m_walkSpeed = float.Parse(vars["Speed"]);
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

            Vector2 velocity = m_target.Value - npc.Position;
            velocity.Normalize();
            velocity *= m_walkSpeed;

            npc.Velocity = velocity;

            npc.Position += velocity * (float)dt.TotalSeconds;
        }

        private Vector2 GetRandomTarget(Vector2 origin, float radius)
        {
            double theta = m_rng.NextDouble() * Math.PI * 2;
            double r = m_rng.NextDouble() * radius;
            return origin + new Vector2((float)(Math.Sin(theta) * r), (float)(Math.Cos(theta) * r));
        }
    }
}
