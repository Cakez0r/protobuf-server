using Data.Abilities;
using Server.Utility;
using Server.Zones;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public sealed class AbilityInstance : IDisposable
    {
        public AbilityModel Ability { get; private set; }

        public IEntity Source { get; private set; }

        public IEntity Target { get; private set; }

        public int StartTime { get; private set; }

        public int EndTime { get; private set; }

        public AbilityState State { get; private set; }

        private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
        public CancellationTokenSource CancellationTokenSource
        {
            get { return m_cancellationTokenSource;  }
        }

        public AbilityInstance(IEntity source, IEntity target, AbilityModel ability)
        {
            Source = source;
            Target = target;
            Ability = ability;

            StartTime = Environment.TickCount;
            EndTime = StartTime + (ability != null ? ability.CastTimeMS : 0);

            if (StartTime == EndTime)
            {
                State = AbilityState.Finished;
            }
            else
            {
                
                State = AbilityState.Casting;
            }
        }

        public async Task<UseAbilityResult> RunAbility()
        {
            UseAbilityResult result = UseAbilityResult.OK;

            try
            {
                //REMEMBER: This runs in the context of the SOURCE.
                //Any checks on the target will need to be awaited on the target's fiber.

                if (State == AbilityState.Casting)
                {
                    await Task.Delay(EndTime - Environment.TickCount, m_cancellationTokenSource.Token);
                }

                if (Vector2.DistanceSquared(Target.Position, Source.Position) > Math.Pow(Ability.Range, 2))
                {
                    result = UseAbilityResult.OutOfRange;
                }
                else if (Source.Power < Ability.SourcePowerDelta)
                {
                    result = UseAbilityResult.NotEnoughPower;
                }
                else if (Target.IsDead)
                {
                    result = UseAbilityResult.InvalidTarget;
                }
                else
                {
                    Target.ApplyHealthDelta(Ability.TargetHealthDelta, Source);
                    Target.ApplyPowerDelta(Ability.TargetPowerDelta, Source);
                    Source.ApplyHealthDelta(Ability.SourceHealthDelta, Source);
                    Source.ApplyPowerDelta(Ability.SourcePowerDelta, Source);

                    result = UseAbilityResult.OK;
                }

                State = AbilityState.Finished;
            }
            catch (TaskCanceledException)
            {
                State = AbilityState.Cancelled;
                result = UseAbilityResult.Cancelled;
            }

            return result;
        }

        public void Dispose()
        {
            m_cancellationTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
