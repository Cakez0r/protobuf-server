
namespace Server.Abilities
{
    public enum UseAbilityResult
    {
        OK = 0,
        Failed = 1,
        NotEnoughPower = 2,
        InvalidTarget = 3,
        OnCooldown = 4,
        OutOfRange = 5,
        AlreadyCasting = 6,
        Cancelled = 7
    }
}
