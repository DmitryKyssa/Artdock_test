using UnityEngine;

public class StatusEffectData: ScriptableObject
{
    [ReadOnly] public string StatusEffectName;
    public Sprite Sprite;
    [Multiline] public string Description;
    public StatusEffectType StatusEffectType;
    public float Duration;
    public float TickRate;
    public float TickValue;
    public float ChanceToApply;
    public float ChanceToResist;
    public float CooldownAfterApplying;
    public Condition Condition;
    public int ConditionValue;
}