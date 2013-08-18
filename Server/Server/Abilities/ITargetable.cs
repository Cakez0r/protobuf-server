using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface ITargetable
    {
        Task<UseAbilityResult> AcceptAbilityAsSource(AbilityInstance abilityInstance);
        Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance abilityInstance);
    }
}
