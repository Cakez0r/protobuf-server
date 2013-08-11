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
        int ID { get; }
        int Health { get; set; }
        int MaxHealth { get; set; }
        Vector2 Position { get; set; }

        float GetStatValue(int statID);

        Future<UseAbilityResult> RunAbilityMutation(Func<ITargetable, UseAbilityResult> mutator);
    }
}
