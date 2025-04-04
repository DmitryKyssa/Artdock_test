using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityData : ScriptableObject
{
    [ReadOnly] public string AbilityName;
    public Sprite Sprite;
    [Multiline] public string Description;
    public TargetType TargetType;
    public Zone Zone;
    public float AreaOfEffectRadius;
    public float CastTime;
    public int ResourceCost;
    public AffectedResourceType AffectedResource;
    public int AffectedResourceValue;
    public int ReceivedResourceValue;
    public float CooldownAfterUsing;
    public Condition Condition;
    public int ConditionValue;
    public List<AnimationData> AnimationDatas = new List<AnimationData>();
    public SFXData SFXData;
    public List<VFXData> VFXDatas = new List<VFXData>();
    public StatusEffectData StatusEffectData;

    public IEnumerator CastAbility(AbilityContext context)
    {
        context.Caster.DeductResource(ResourceCost);

        if (AnimationDatas != null && AnimationDatas.Count > 0)
        {
            foreach (AnimationData animationData in AnimationDatas)
            {
                if (animationData.TargetType == TargetType.Self)
                {
                    animationData.PlayAnimation(context.Caster);
                }
                else
                {
                    foreach (Unit target in context.Targets)
                    {
                        animationData.PlayAnimation(target);
                    }
                }
            }
        }

        if (VFXDatas != null && VFXDatas.Count > 0)
        {
            foreach (VFXData vfxData in VFXDatas)
            {
                if (vfxData.TargetType == TargetType.Self)
                {
                    CoroutineRunner.Instance.Run(vfxData.PlayVFX(context.Caster));
                }
                else
                {
                    foreach (Unit target in context.Targets)
                    {
                        CoroutineRunner.Instance.Run(vfxData.PlayVFX(target));
                    }
                }
            }
        }

        if (SFXData != null)
        {
            CoroutineRunner.Instance.Run(SFXData.PlaySFX(context.Caster));
        }

        if (StatusEffectData != null)
        {
            if(StatusEffectData.TargetType == TargetType.Self)
            {
                context.Caster.ApplyEffect(StatusEffectData);
            }
            else
            {
                foreach (Unit target in context.Targets)
                {
                    target.ApplyEffect(StatusEffectData);
                }
            }
        }

        switch (TargetType)
        {
            case TargetType.Self:
                context.Caster.AffectResource(AffectedResource, AffectedResourceValue);
                context.Caster.RecievedResource(ReceivedResourceValue);
                break;
            case TargetType.Enemy or TargetType.Ally or TargetType.Allies or TargetType.All or TargetType.Enemies:
                context.Caster.CreateEffectZone(Zone, AreaOfEffectRadius);
                foreach (Unit target in context.Targets)
                {
                    target.AffectResource(AffectedResource, AffectedResourceValue);
                    target.RecievedResource(ReceivedResourceValue);
                }      
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }

        UIManager.Instance.StartAbilityCooldown(AbilityName);

        yield return new WaitForSeconds(CastTime);
    }
}