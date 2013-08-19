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
        private static readonly TimeSpan m_globalCooldown = TimeSpan.FromMilliseconds(1000);
        private IReadOnlyDictionary<int, float> m_stats;

        private IAbilityRepository m_abilityRepository;
        private CancellationTokenSource m_spellCastCancellationToken;

        private DateTime m_lastAbilityAcceptTime = DateTime.Now;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            AbilityModel abilityModel = m_abilityRepository.GetAbilityByID(ability.AbilityID);
            UseAbilityResult result = UseAbilityResult.Failed;

            //If not already casting and ability is valid
            if (m_spellCastCancellationToken != null && abilityModel != null)
            {
                //Not on global cooldown
                if (DateTime.Now - m_lastAbilityAcceptTime > m_globalCooldown)
                {
                    m_lastAbilityAcceptTime = DateTime.Now;

                    //Send response and create cancellation token if ability has a cast time
                    if (abilityModel.CastTimeMS > 0)
                    {
                        Send(new UseAbility_S2C() { Result = (int)result, Timestamp = Environment.TickCount });

                        m_spellCastCancellationToken = new CancellationTokenSource();
                        await Task.Delay(abilityModel.CastTimeMS, m_spellCastCancellationToken.Token);
                    }

                    //Resolve the target if the ability is being used on a target
                    ITargetable target = default(ITargetable);
                    if (ability.TargetID != 0)
                    {
                        target = await CurrentZone.GetTarget(ability.TargetID);
                        if (target == null)
                        {
                            result = UseAbilityResult.InvalidTarget;
                        }
                    }

                    //If the target was found or the ability isn't being used on a target...
                    if (target != null || ability.TargetID == 0)
                    {
                        //Create the ability instance
                        AbilityInstance abilityInstance = new AbilityInstance(this, target, abilityModel);

                        //Run deltas on source
                        result = AcceptAbilityAsSource(abilityInstance);

                        //Run deltas on target if the source deltas were ok and there is a target
                        if (target != null && result == UseAbilityResult.Completed)
                        {
                            result = await target.AcceptAbilityAsTarget(abilityInstance);
                        }
                    }
                }
                else
                {
                    result = UseAbilityResult.OnCooldown;
                }
            }

            Respond(ability, new UseAbility_S2C() { Result = (int)result, Timestamp = Environment.TickCount });
            m_spellCastCancellationToken = null;
        }

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            CurrentZone.PlayerUsedAbility(ability);
            return UseAbilityResult.Completed;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                return UseAbilityResult.Completed;
            });
        }
    }
}
