public interface IEffectable
{
    void ApplyEffect(StatusEffectData statusEffect);
    void RemoveEffect(StatusEffectData statusEffect);
}

public interface IAbilitiable
{
    void CastAbility(AbilityContext abilityContext);
}