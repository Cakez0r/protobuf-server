using System;

namespace Server.Gameplay
{
    public class Formulas
    {
        public static int StaminaToHealth(float stamina)
        {
            return (int)stamina * 25;
        }

        public static byte XPToLevel(float xp)
        {
            return (byte)(Math.Pow(xp + 9, 1.0 / 3) - 2);
        }

        public static int LevelToXP(float level)
        {
            return (int)Math.Pow(level + 2, 3) - 8;
        }

        public static int LevelToPower(byte level)
        {
            return 100 + level * 20;
        }
    }
}
