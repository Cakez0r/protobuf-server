using Data.Abilities;
using Data.Players;
using Protocol;
using Server.Abilities;
using Server.Player;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public partial class PlayerPeer
    {
        private static readonly TimeSpan m_globalCooldown = TimeSpan.FromMilliseconds(1000);
        private IReadOnlyDictionary<StatType, PlayerStatModel> m_stats;

        private IAbilityRepository m_abilityRepository;
        private CancellationTokenSource m_spellCastCancellationToken;

        private DateTime m_lastAbilityAcceptTime = DateTime.Now;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            AbilityModel abilityModel = m_abilityRepository.GetAbilityByID(ability.AbilityID);
            UseAbilityResult result = UseAbilityResult.Failed;

            Trace("Use ability ID {0}", ability.AbilityID);

            //If not already casting and ability is valid
            if (m_spellCastCancellationToken == null && abilityModel != null)
            {
                //Not on global cooldown
                if (DateTime.Now - m_lastAbilityAcceptTime > m_globalCooldown)
                {
                    try
                    {
                        m_lastAbilityAcceptTime = DateTime.Now;
                        result = UseAbilityResult.Accepted;

                        //Send response and create cancellation token if ability has a cast time
                        if (abilityModel.CastTimeMS > 0)
                        {
                            m_spellCastCancellationToken = new CancellationTokenSource();

                            Send(new AbilityUseStarted() { Result = (int)result, FinishTime = Environment.TickCount + abilityModel.CastTimeMS, Timestamp = Environment.TickCount });

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

                            m_spellCastCancellationToken = null;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        result = UseAbilityResult.Cancelled;
                        Trace("Use ability cancelled");
                    }
                }
                else
                {
                    result = UseAbilityResult.OnCooldown;
                }
            }
            else
            {
                result = UseAbilityResult.AlreadyCasting;
            }

            Respond(ability, new UseAbility_S2C() { Result = (int)result, Timestamp = Environment.TickCount });
        }

        private void Handle_StopCasting(StopCasting sc)
        {
            StopCasting(false);
        }

        private void StopCasting(bool notifyClient)
        {
            if (m_spellCastCancellationToken != null)
            {
                Trace("Stop casting");
                m_spellCastCancellationToken.Cancel();
                m_spellCastCancellationToken = null;
            }
        }

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            Trace("Accepting ability as source");
            CurrentZone.PlayerUsedAbility(ability);

            UseAbilityResult result = UseAbilityResult.Completed;

            ApplyHealthDelta(ability.Ability.SourceHealthDelta);
            ApplyPowerDelta(ability.Ability.SourcePowerDelta);

            return result;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                Trace("Accepting ability as target");

                UseAbilityResult result = UseAbilityResult.OutOfRange;
                if (Vector2.DistanceSquared(ability.Source.Position, Position) <= ability.Ability.Range * ability.Ability.Range)
                {
                    result = UseAbilityResult.Completed;

                    ApplyHealthDelta(ability.Ability.TargetHealthDelta);
                    ApplyPowerDelta(ability.Ability.TargetPowerDelta);
                }

                return result;
            });
        }
    }
}
