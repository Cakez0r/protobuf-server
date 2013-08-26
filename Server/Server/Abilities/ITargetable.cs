using Server.Utility;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        string Name { get; }
        Vector2 Position { get; }
        byte Level { get; }
        int Health { get; }
        int Power { get; }
        int MaxHealth { get; }
        int MaxPower { get; }
        bool IsDead { get; }

        void ApplyHealthDelta(int delta, ITargetable source);
        void ApplyPowerDelta(int delta, ITargetable source);
        void ApplyXPDelta(int delta, ITargetable source);
    }
}
