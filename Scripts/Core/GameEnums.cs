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

    public enum ProjectileSpreadMode
    {
        FanAim,   // fan toward nearest enemy (default)
        Radial    // evenly spaced 360° ring (Flak)
    }
}
