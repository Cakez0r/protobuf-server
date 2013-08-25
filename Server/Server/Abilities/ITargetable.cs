using Server.Gameplay;
using Server.Utility;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        string Name { get; }
        Vector2 Position { get; }
        byte Level { get; }

        UseAbilityResult AcceptAbilityAsSource(AbilityInstance abilityInstance);
        Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance abilityInstance);

        void AwardXP(float xp);
    }
}
