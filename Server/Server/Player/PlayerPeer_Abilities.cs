using Protocol;
using Server.Abilities;
using System.Collections.Generic;

namespace Server
{
    public partial class PlayerPeer
    {
        private IReadOnlyDictionary<int, float> m_stats;

        private void Handle_UseAbility(UseAbility_C2S ability)
        {
            CurrentZone.PlayerUseAbility(this, ability.TargetID, ability.AbilityID);
        }

        public void AcceptAbility(AbilityInstance ability)
        {

        }
    }
}
