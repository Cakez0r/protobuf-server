using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Gameplay
{
    public class Formulas
    {
        public static int StaminaToHealth(float stamina)
        {
            return (int)stamina * 25;
        }
    }
}
