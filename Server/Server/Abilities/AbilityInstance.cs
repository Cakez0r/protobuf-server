using Data.Abilities;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public class AbilityInstance
    {
        public AbilityModel Ability
        {
            get;
            private set; 
        }

        public ITargetable Source
        {
            get;
            private set;
        }

        public ITargetable Target
        {
            get;
            private set;
        }

        public Future<UseAbilityResult> Result
        {
            get;
            private set;
        }

        public AbilityInstance(ITargetable source, ITargetable target, AbilityModel ability, Future<UseAbilityResult> resultFuture)
        {
            Source = source;
            Target = target;
            Ability = ability;
            Result = resultFuture;
        }
    }
}
