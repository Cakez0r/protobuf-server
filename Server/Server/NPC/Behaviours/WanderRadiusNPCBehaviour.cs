using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.NPC.Behaviours
{
    public class WanderRadiusNPCBehaviour : INPCBehaviour
    {
        private float m_walkSpeed;

        private float m_radius;

        private Vector2? m_target;

        private Random m_rng;

        private List<Vector2> m_currentPath = new List<Vector2>();
        private int m_currentWaypoint;
        private Vector2 m_pathTarget;


        public void Initialise(IReadOnlyDictionary<string, string> vars)
        {
            m_rng = new Random(GetHashCode());
            m_radius = float.Parse(vars["Radius"]);
            m_walkSpeed = float.Parse(vars["Speed"]);
        }

        public void Update(TimeSpan dt, NPCInstance npc)
        {
            if (m_target == null || Vector2.Distance(m_target.Value, npc.Position) < 4)
            {
                m_target = GetRandomTarget(new Vector2((float)npc.NPCSpawnModel.X, (float)npc.NPCSpawnModel.Y), m_radius);
                m_currentPath = null;
            }

            if (m_target.Value != m_pathTarget)
            {
                while (m_currentPath == null || m_currentPath.Count == 0)
                {
                    m_target = GetRandomTarget(new Vector2((float)npc.NPCSpawnModel.X, (float)npc.NPCSpawnModel.Y), m_radius);
                    m_currentPath = Program.Map.CalculatePath(npc.Position, m_target.Value);
                }
                m_currentWaypoint = 0;

                m_pathTarget = m_target.Value;
            }

            while (Vector2.Distance(npc.Position, m_currentPath[m_currentWaypoint]) < 1)
            {
                m_currentWaypoint++;

                if (m_currentWaypoint >= m_currentPath.Count)
                {
                    return;
                }
            }

            Vector2 velocity = m_currentPath[m_currentWaypoint] - npc.Position;
            if (velocity == Vector2.Zero)
            {
                return;
            }
            velocity.Normalize();
            velocity *= m_walkSpeed;

            npc.Velocity = velocity;

            npc.Position += velocity * (float)dt.TotalSeconds;
        }

        private Vector2 GetRandomTarget(Vector2 origin, float radius)
        {
            Vector2 position = Vector2.Zero;
            do
            {
                double theta = m_rng.NextDouble() * Math.PI * 2;
                double r = m_rng.NextDouble() * radius;
                position = origin + new Vector2((float)(Math.Sin(theta) * r), (float)(Math.Cos(theta) * r));
            } while (Program.Map.CollisionAreas.Any(c => c.ContainsPoint(position)));

            return position;
        }
    }
}
