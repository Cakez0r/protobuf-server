using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Players
{
    public class PlayerStatModel
    {
        public int PlayerStatID { get; set; }
        public int PlayerID { get; set; }
        public int StatID { get; set; }
        public float StatValue { get; set; }
    }
}
