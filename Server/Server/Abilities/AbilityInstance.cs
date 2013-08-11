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
        private List<IAbilityBehaviour> m_behaviours = new List<IAbilityBehaviour>();

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

        public AbilityInstance(ITargetable source, ITargetable target, List<IAbilityBehaviour> behaviours)
        {
            m_behaviours = behaviours;
            Source = source;
            Target = target;
            Result = new Future<UseAbilityResult>();
        }

        public void Update(TimeSpan dt)
        {
            foreach (IAbilityBehaviour behaviour in m_behaviours)
            {
                behaviour.Update(dt, this);
            }
        }
    }
}
