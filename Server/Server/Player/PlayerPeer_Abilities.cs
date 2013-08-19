using Data.Abilities;
using Protocol;
using Server.Abilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public partial class PlayerPeer
    {
        private IReadOnlyDictionary<int, float> m_stats;

        private IAbilityRepository m_abilityRepository;
        private CancellationTokenSource m_spellCastCancellationToken;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            AbilityModel abilityModel = m_abilityRepository.GetAbilityByID(ability.AbilityID);
            UseAbilityResult result = UseAbilityResult.Failed;

            if (m_spellCastCancellationToken != null && abilityModel != null)
            {
                //Leaves fiber from here
                m_spellCastCancellationToken = new CancellationTokenSource();
                await Task.Delay(abilityModel.CastTimeMS, m_spellCastCancellationToken.Token);

                ITargetable target = default(ITargetable);
                if (ability.TargetID != 0)
                {
                    target = await CurrentZone.GetTarget(ability.TargetID);
                }

                AbilityInstance abilityInstance = new AbilityInstance(this, target, abilityModel);

                result = await target.AcceptAbilityAsTarget(abilityInstance);
                if (result == UseAbilityResult.OK)
                {
                    result = await AcceptAbilityAsSource(abilityInstance);
                }
            }

            Respond(ability, new UseAbility_S2C() { Result = (int)result });
            m_spellCastCancellationToken = null;
        }

        public Task<UseAbilityResult> AcceptAbilityAsSource(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                CurrentZone.PlayerUsedAbility(ability);
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
