using UnityEngine;

public class StatusEffectData: ScriptableObject
{
    [ReadOnly] public string StatusEffectName;
    [Multiline] public string Description;
    public TargetType TargetType;
    [ReadOnly] public bool IsEndless;
    [ReadOnly] public int Duration;
    [ReadOnly] public bool IsPeriodic;
    [ReadOnly] public float Period;
    public AffectedResourceType AffectedResource;
    public float AffectedResourceValuePerPeriod;
}