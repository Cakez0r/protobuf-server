using Protocol;
using Server.Abilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server
{
    public partial class PlayerPeer
    {
        private IReadOnlyDictionary<int, float> m_stats;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            UseAbilityResult result = await CurrentZone.PlayerUseAbility(this, ability.TargetID, ability.AbilityID);
            await Fiber.Enqueue(() => Respond(ability, new UseAbility_S2C() { Result = (int)result }));
        }

        public Task<UseAbilityResult> AcceptAbilityAsSource(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                return UseAbilityResult.OK;
            });
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                return UseAbilityResult.OK;
            });
        }
    }
}
