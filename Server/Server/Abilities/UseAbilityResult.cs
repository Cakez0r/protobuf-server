
namespace Server.Abilities
{
    public enum UseAbilityResult
    {
        Completed = 0,
        Failed = 1,

        NotEnoughPower = 2,
        InvalidTarget = 3,
        OnCooldown = 4,
        OutOfRange = 5,
        Cancelled = 6,
        AlreadyCasting = 7
    }
}
