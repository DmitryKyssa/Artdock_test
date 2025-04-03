public enum AbilityTargetType
{
    Self,
    Enemy,
    Enemies,
    Ally,
    Allies,
    All
}

public enum AbilityEffectZone
{
    AutoAreaOfEffect,
    CustomAreaOfEffect,
    SingleTarget,
    AllLocation
}

public enum SpentResourceType
{
    None,
    Stamina,
    HP
}

public enum AppliedResourceType
{
    None,
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
    Stamina
}

public enum StatusEffectType
{
    //TODO: Add more status effects
}