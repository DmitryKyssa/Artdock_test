public enum TargetType
{
    Self,
    Enemy,
    Enemies,
    Ally,
    Allies,
    All
}

public enum EffectZone
{
    SingleTarget,
    AutoAreaOfEffect,
    CustomAreaOfEffect,
    AllLocation
}

public enum SpentResourceType
{
    Stamina,
    HP
}

public enum AffectedResourceType
{
    HP,
    Stamina,
    MovementSpeed,
    AttackDamage
}

public enum ReceivedResourceType
{
    None,
    HP,
    Stamina,
    MovementSpeed,
    AttackDamage,
    XP
}

public enum Condition
{
    None,
    XP,
}