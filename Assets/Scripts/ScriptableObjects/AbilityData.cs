using System.Collections.Generic;
using UnityEngine;

public class AbilityData : ScriptableObject
{
    [ReadOnly] public string AbilityName;
    public Sprite Sprite;
    [Multiline] public string Description;
    public TargetType TargetType;
    public EffectZone EffectZone;
    public float AreaOfEffectRadius;
    public float CastTime;
    public SpentResourceType SpentResource;
    public int ResourceCost;
    public AffectedResourceType AffectedResource;
    public int AffectedResourceValue;
    public ReceivedResourceType ReceivedResource;
    public int ReceivedResourceValue;
    public float CooldownAfterUsing;
    public Condition Condition;
    public int ConditionValue;
    public List<AnimationData> AnimationDatas = new List<AnimationData>();
    public SFXData SFXData;
    public List<VFXData> VFXDatas = new List<VFXData>();
    public List<StatusEffectData> StatusEffectDatas = new List<StatusEffectData>();
}