using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abilities
{
    public class AbilityBehaviourModel
    {
        public int AbilityBehaviourID { get; set; }
        public int AbilityID { get; set; }
        public string BehaviourType { get; set; }
        public int ExecutionOrder { get; set; }
    }
}
