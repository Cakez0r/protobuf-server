using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        void AcceptAbility(AbilityInstance abilityInstance);
    }
}
