using UnityEngine;

public class StatusEffectData: ScriptableObject
{
    [ReadOnly] public string StatusEffectName;
    [Multiline] public string Description;
    public TargetType TargetType;
    public bool IsEndless;
    public int Duration;
    public bool IsPeriodic;
    public float Period;
    public AffectedResourceType AffectedResource;
    public float AffectedResourceValuePerPeriod;
}