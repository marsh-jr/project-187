namespace Project187
{
    public enum AttackType
    {
        Projectile,
        Area,
        Beam,
        Melee
    }

    public enum DamageType
    {
        Kinetic,
        Energy,
        Explosive
    }

    public enum AdaptationCategory
    {
        Projectile = 0,
        Area       = 1,
        Beam       = 2,
        Melee      = 3,
        Universal  = 99
    }
}
