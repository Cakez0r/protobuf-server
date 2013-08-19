using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        UseAbilityResult AcceptAbilityAsSource(AbilityInstance abilityInstance);
        Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance abilityInstance);
    }
}
