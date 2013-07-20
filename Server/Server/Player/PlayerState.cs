using Protocol;
using Server.Utility;
using Server.Zones;
using System.Collections.Generic;

namespace Server
{
    public class PlayerState
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }

        public int? TargetID { get; set; }

        public int TimeOnClient { get; set; }

        public Zone CurrentZone { get; set; }

        public WorldState WorldState { get; set; }

        public PlayerState()
        {
            WorldState = new WorldState() { PlayerStates = new List<PlayerStateUpdate_S2C>() };
        }
    }
}
