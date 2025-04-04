using System;
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
        List<Unit> allies = new List<Unit>();
        List<Unit> enemies = new List<Unit>();
        if (context.Caster.gameObject.layer == UnitsSpawner.Instance.FirstTeamLayerValue)
        {
            allies.AddRange(UnitsSpawner.Instance.FirstTeamUnits);
            enemies.AddRange(UnitsSpawner.Instance.SecondTeamUnits);
        }
        else
        {
            allies.AddRange(UnitsSpawner.Instance.SecondTeamUnits);
            enemies.AddRange(UnitsSpawner.Instance.FirstTeamUnits);
        }

        context.Caster.DeductResource(ResourceCost);

        if (AnimationDatas != null && AnimationDatas.Count > 0)
        {
            foreach (AnimationData animationData in AnimationDatas)
            {
                switch (animationData.TargetType)
                {
                    case TargetType.Self:
                        animationData.PlayAnimation(context.Caster);
                        break;
                    case TargetType.Allies:
                        foreach (Unit ally in allies)
                        {
                            animationData.PlayAnimation(ally);
                        }
                        break;
                    case TargetType.Enemies:
                        foreach (Unit enemy in enemies)
                        {
                            animationData.PlayAnimation(enemy);
                        }
                        break;
                    case TargetType.All:
                        foreach (Unit ally in allies)
                        {
                            animationData.PlayAnimation(ally);
                        }
                        foreach (Unit enemy in enemies)
                        {
                            animationData.PlayAnimation(enemy);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(animationData.TargetType), animationData.TargetType, null);
                }
            }
        }

        if (VFXDatas != null && VFXDatas.Count > 0)
        {
            foreach (VFXData vfxData in VFXDatas)
            {
                switch (vfxData.TargetType)
                {
                    case TargetType.Self:
                        CoroutineRunner.Instance.Run(vfxData.PlayVFX(context.Caster));
                        break;
                    case TargetType.Allies:
                        foreach (Unit ally in allies)
                        {
                            CoroutineRunner.Instance.Run(vfxData.PlayVFX(ally));
                        }
                        break;
                    case TargetType.Enemies:
                        foreach (Unit enemy in enemies)
                        {
                            CoroutineRunner.Instance.Run(vfxData.PlayVFX(enemy));
                        }
                        break;
                    case TargetType.All:
                        foreach (Unit ally in allies)
                        {
                            CoroutineRunner.Instance.Run(vfxData.PlayVFX(ally));
                        }
                        foreach (Unit enemy in enemies)
                        {
                            CoroutineRunner.Instance.Run(vfxData.PlayVFX(enemy));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(vfxData.TargetType), vfxData.TargetType, null);
                }
            }
        }

        if (SFXData != null)
        {
            CoroutineRunner.Instance.Run(SFXData.PlaySFX(context.Caster));
        }

        context.Caster.RecievedResource(ReceivedResourceValue);

        switch (TargetType)
        {
            case TargetType.Self:
                context.Caster.AffectResource(AffectedResource, AffectedResourceValue);
                break;
            case TargetType.Allies:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius); 
                foreach (Unit ally in allies)
                {
                    Debug.Log($"Ally {ally.name} is in ability zone: {ally.IsInAbilityZone}");
                    if (ally.IsInAbilityZone)
                    {
                        ally.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            case TargetType.Enemies:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius);
                foreach (Unit enemy in enemies)
                {
                    if (enemy.IsInAbilityZone)
                    {
                        enemy.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            case TargetType.All:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius);
                foreach (Unit ally in allies)
                {
                    if (ally.IsInAbilityZone)
                    {
                        ally.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                foreach (Unit enemy in enemies)
                {
                    if (enemy.IsInAbilityZone)
                    {
                        enemy.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(TargetType), TargetType, null);
        }

        if (StatusEffectData != null)
        {
            switch (StatusEffectData.TargetType)
            {
                case TargetType.Self:
                    context.Caster.ApplyEffect(StatusEffectData);
                    break;
                case TargetType.Allies:
                    foreach (Unit ally in allies)
                    {
                        ally.ApplyEffect(StatusEffectData);
                    }
                    break;
                case TargetType.Enemies:
                    foreach (Unit enemy in enemies)
                    {
                        enemy.ApplyEffect(StatusEffectData);
                    }
                    break;
                case TargetType.All:
                    foreach (Unit ally in allies)
                    {
                        ally.ApplyEffect(StatusEffectData);
                    }
                    foreach (Unit enemy in enemies)
                    {
                        enemy.ApplyEffect(StatusEffectData);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(StatusEffectData.TargetType), StatusEffectData.TargetType, null);
            }
        }

        UIManager.Instance.StartAbilityCooldown(AbilityName);

        yield return new WaitForSeconds(CastTime);
        context.Caster.DestroyZone();

        foreach (Unit ally in allies)
        {
            ally.IsInAbilityZone = false;
        }
        foreach (Unit enemy in enemies)
        {
            enemy.IsInAbilityZone = false;
        }

        Debug.Log($"Ability {AbilityName} casted by {context.Caster.name}");
    }
}