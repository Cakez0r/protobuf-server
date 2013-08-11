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
        IReadOnlyDictionary<int, float> m_stats;

        public float GetStatValue(int statID)
        {
            float value = default(float);

            m_stats.TryGetValue(statID, out value);

            return value;
        }

        public Future<UseAbilityResult> RunAbilityMutation(Func<ITargetable, UseAbilityResult> mutator)
        {
            return m_accessor.Transaction<UseAbilityResult>(mutator);
        }
    }
}
