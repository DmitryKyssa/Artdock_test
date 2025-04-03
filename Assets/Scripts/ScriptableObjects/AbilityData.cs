using UnityEngine;

public class AbilityData : ScriptableObject
{
    [ReadOnly] public string AbilityName;
    public Sprite Sprite;
    [Multiline] public string Description;
    public AbilityTargetType TargetType;
    public AbilityEffectZone EffectZone;
    public float AreaOfEffectRadius;
    public float CastTime;
    public SpentResourceType SpentResource;
    public int ResourceCost;
    public AppliedResourceType AppliedResource;
    public int AppliedResourceValue;
    public ReceivedResourceType ReceivedResource;
    public int ReceivedResourceValue;
    public float CooldownAfterUsing;
    public Condition Condition;
    public int ConditionValue;
}