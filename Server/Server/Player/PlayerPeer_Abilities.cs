using Protocol;
using Server.Abilities;
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
        private IReadOnlyDictionary<int, float> m_stats;

        private void Handle_UseAbility(UseAbility_C2S ability)
        {
            CurrentZone.PlayerUseAbility(this, ability.TargetID, ability.AbilityID);
        }

        public float GetStatValue(int statID)
        {
            float value = default(float);

            m_stats.TryGetValue(statID, out value);

            return value;
        }

        public void AcceptAbility(AbilityInstance ability)
        {

        }
    }
}
