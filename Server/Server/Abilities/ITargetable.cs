using Server.Utility;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        string Name { get; }
        Vector2 Position { get; }

        UseAbilityResult AcceptAbilityAsSource(AbilityInstance abilityInstance);
        Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance abilityInstance);
    }
}
