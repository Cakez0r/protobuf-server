using Protocol;
using Server.Utility;
using System.Threading.Tasks;

namespace Server.Zones
{
    public interface IEntity
    {
        int ID { get; }
        string Name { get; }
        Vector2 Position { get; }
        byte Level { get; }
        int Health { get; }
        int Power { get; }
        int MaxHealth { get; }
        int MaxPower { get; }
        bool IsDead { get; }

        void ApplyHealthDelta(int delta, IEntity source);
        void ApplyPowerDelta(int delta, IEntity source);
        void ApplyXPDelta(int delta, IEntity source);

        EntityStateUpdate GetStateUpdate();
    }
}
