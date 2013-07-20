using Server.Utility;

namespace Server
{
    public class PlayerState
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }

        public int? TargetID { get; set; }

        public int TimeOnClient { get; set; }
    }
}
