
namespace Server.Abilities
{
    public enum UseAbilityResult
    {
        Completed = 0,
        Failed = 1,

        Accepted = 2,
        NotEnoughPower = 3,
        InvalidTarget = 4,
        OnCooldown = 5,
        OutOfRange = 6
    }
}
