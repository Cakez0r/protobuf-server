using Data.Abilities;
using Data.Players;
using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public partial class PlayerPeer
    {
        private const int GLOBAL_COOLDOWN_MS = 1000;

        private IReadOnlyDictionary<StatType, PlayerStatModel> m_stats;

        private IAbilityRepository m_abilityRepository;
        private CancellationTokenSource m_spellCastCancellationToken;

        private int m_lastAbilityAcceptTime = Environment.TickCount;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            AbilityModel abilityModel = m_abilityRepository.GetAbilityByID(ability.AbilityID);
            UseAbilityResult result = UseAbilityResult.Failed;

            string targetName = "[No Target]";

            if (abilityModel == null)
            {
                result = UseAbilityResult.Failed;
            }
            else if (m_spellCastCancellationToken != null)
            {
                result = UseAbilityResult.AlreadyCasting;
            }
            else if (Environment.TickCount - m_lastAbilityAcceptTime <= GLOBAL_COOLDOWN_MS)
            {
                result = UseAbilityResult.OnCooldown;
            }
            else if (Power + abilityModel.SourcePowerDelta < 0)
            {
                result = UseAbilityResult.NotEnoughPower;
            }
            else
            {
                ITargetable target = default(ITargetable);
                if (ability.TargetID != 0)
                {
                    target = await CurrentZone.GetTarget(ability.TargetID);
                    if (target != null)
                    {
                        targetName = target.Name;
                    }
                }

                if (abilityModel.AbilityType != AbilityModel.ETargetType.AOE && target == null)
                {
                    result = UseAbilityResult.InvalidTarget;
                }
                else if (Vector2.DistanceSquared(target.Position, Position) > Math.Pow(abilityModel.Range, 2))
                {
                    result = UseAbilityResult.OutOfRange;
                }
                else
                {
                    try
                    {
                        if (abilityModel.CastTimeMS > 0)
                        {
                            m_spellCastCancellationToken = new CancellationTokenSource();

                            Trace("Started casting");

                            Send(new AbilityUseStarted() { Result = (int)result, FinishTime = Environment.TickCount + abilityModel.CastTimeMS, Timestamp = Environment.TickCount });

                            await Task.Delay(abilityModel.CastTimeMS, m_spellCastCancellationToken.Token);
                        }

                        AbilityInstance abilityInstance = new AbilityInstance(this, target, abilityModel);

                        result = AcceptAbilityAsSource(abilityInstance);

                        if (result == UseAbilityResult.Completed)
                        {
                            result = await target.AcceptAbilityAsTarget(abilityInstance);
                        }

                        m_spellCastCancellationToken = null;
                    }
                    catch (TaskCanceledException)
                    {
                        result = UseAbilityResult.Cancelled;
                    }
                }
            }

            string abilityName = abilityModel != null ? abilityModel.InternalName : string.Format("[INVALID ID {0}]", ability.AbilityID);
            Info("Used ability {0} on target {1} with result {2}", abilityName, targetName, result);

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
                Trace("Stopped casting");
                m_spellCastCancellationToken.Cancel();
                m_spellCastCancellationToken = null;
            }
        }

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            CurrentZone.PlayerUsedAbility(ability);

            UseAbilityResult result = UseAbilityResult.Failed;

            if (Power + ability.Ability.SourcePowerDelta < 0)
            {
                result = UseAbilityResult.NotEnoughPower;
            }
            else
            {
                ApplyHealthDelta(ability.Ability.SourceHealthDelta);
                ApplyPowerDelta(ability.Ability.SourcePowerDelta);
                result = UseAbilityResult.Completed;
            }

            return result;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return Fiber.Enqueue(() =>
            {
                UseAbilityResult result = UseAbilityResult.Failed;
                if (Vector2.DistanceSquared(ability.Source.Position, Position) > Math.Pow(ability.Ability.Range, 2))
                {
                    result = UseAbilityResult.OutOfRange;
                }
                else
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
