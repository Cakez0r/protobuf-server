﻿using Data.Abilities;
using Data.Players;
using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.NPC;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;

namespace Server
{
    public partial class PlayerPeer
    {
        private const int GLOBAL_COOLDOWN_MS = 1000;

        private IReadOnlyDictionary<StatType, PlayerStatModel> m_stats;

        private IAbilityRepository m_abilityRepository;
        private AbilityInstance m_lastAbility = new AbilityInstance(null, null, null);

        private int m_lastAbilityAcceptTime = Environment.TickCount;

        private async void Handle_UseAbility(UseAbility_C2S ability)
        {
            IEntity target = default(IEntity);
            AbilityModel abilityModel = m_abilityRepository.GetAbilityByID(ability.AbilityID);
            UseAbilityResult result = UseAbilityResult.Failed;

            string targetName = "[No Target]";

            if (abilityModel == null)
            {
                result = UseAbilityResult.Failed;
            }
            else if (m_lastAbility.State == AbilityState.Casting)
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
                else if (target is NPCInstance && abilityModel.AbilityType == AbilityModel.EAbilityType.HELP)
                {
                    result = UseAbilityResult.InvalidTarget;
                }
                else if (Vector2.DistanceSquared(target.Position, Position) > Math.Pow(abilityModel.Range, 2))
                {
                    result = UseAbilityResult.OutOfRange;
                }
                else
                {
                    m_lastAbility.Dispose();
                    m_lastAbility = new AbilityInstance(this, target, abilityModel);

                    if (m_lastAbility.State == AbilityState.Casting)
                    {
                        Send(new AbilityCastNotification() { StartTime = m_lastAbility.StartTime, EndTime = m_lastAbility.EndTime });
                    }

                    result = await m_lastAbility.RunAbility();
                }
            }

            if (result == UseAbilityResult.OK)
            {
                List<IEntity> nearPlayers = m_nearEntities;

                AbilityUsedNotification notification = new AbilityUsedNotification();
                notification.AbilityID = abilityModel.AbilityID;
                notification.SourceID = ID;
                notification.TargetID = target != null ? target.ID : 0;
                notification.Timestamp = Environment.TickCount;

                for (int i = 0; i < nearPlayers.Count; i++)
                {
                    PlayerPeer player = nearPlayers[i] as PlayerPeer;
                    if (player != null)
                    {
                        player.EnqueueSend(notification);
                    }
                }
            }

            string abilityName = abilityModel != null ? abilityModel.InternalName : string.Format("[INVALID ID {0}]", ability.AbilityID);
            Info("Used ability {0} on target {1} with result {2}", abilityName, targetName, result);

            Respond(ability, new UseAbility_S2C() { Result = (int)result });
        }

        private void Handle_StopCasting(StopCasting sc)
        {
            StopCasting();
        }

        private void StopCasting()
        {
            if (m_lastAbility.State == AbilityState.Casting)
            {
                Trace("Stopped casting");
                m_lastAbility.CancellationTokenSource.Cancel();
            }
        }

        private float GetStatValue(StatType statType)
        {
            PlayerStatModel stat;

            if (m_stats.TryGetValue(statType, out stat))
            {
                return stat.StatValue;
            }

            return 0;
        }

        private void SendStatChanges(params StatType[] stats)
        {
            StatsChangedNotification scn = new StatsChangedNotification();
            scn.NewStats = new Dictionary<int, float>();

            foreach (StatType st in stats)
            {
                scn.NewStats.Add((int)st, GetStatValue(st));
            }

            EnqueueSend(scn);
        }
    }
}
