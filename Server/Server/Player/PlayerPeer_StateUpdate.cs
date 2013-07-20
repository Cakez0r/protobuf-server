using Protocol;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public partial class PlayerPeer
    {
        public void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
        {
            m_state.Access((s) =>
            {
                s.Rotation = psu.Rot;
                s.Position = new Vector2(psu.X, psu.Y);
                s.Velocity = new Vector2(psu.X, psu.Y);
                s.TimeOnClient = psu.Time;
                s.TargetID = psu.TargetID;
            });
        }
    }
}
