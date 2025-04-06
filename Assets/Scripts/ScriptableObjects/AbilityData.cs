using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityData : ScriptableObject
{
    [ReadOnly] public string AbilityName;
    public Sprite Sprite;
    [Multiline] public string Description;
    public TargetType TargetType;
    public Zone Zone;
    public float AreaOfEffectRadius;
    public float CustomAreaOfEffectPositioningDuration;
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
        if (context.Caster.UnitTeam == UnitsSpawner.FirstTeamUnitName)
        {
            for (int i = 0; i < UnitsSpawner.Instance.FirstTeamUnits.Count; i++)
            {
                if (UnitsSpawner.Instance.FirstTeamUnits[i] != context.Caster)
                {
                    allies.Add(UnitsSpawner.Instance.FirstTeamUnits[i]);
                }
            }
            enemies.AddRange(UnitsSpawner.Instance.SecondTeamUnits);
        }
        else
        {
            for (int i = 0; i < UnitsSpawner.Instance.SecondTeamUnits.Count; i++)
            {
                if (UnitsSpawner.Instance.SecondTeamUnits[i] != context.Caster)
                {
                    allies.Add(UnitsSpawner.Instance.SecondTeamUnits[i]);
                }
            }
            enemies.AddRange(UnitsSpawner.Instance.FirstTeamUnits);
        }

        context.Caster.DeductResource(ResourceCost);

        Unit oneTarget = null;
        if (TargetType == TargetType.Enemy)
        {
            oneTarget = enemies[Random.Range(0, enemies.Count)];
        }
        else if (TargetType == TargetType.Ally)
        {
            oneTarget = allies[Random.Range(0, allies.Count)];
        }

        if (AnimationDatas != null && AnimationDatas.Count > 0)
        {
            foreach (AnimationData animationData in AnimationDatas)
            {
                switch (animationData.TargetType)
                {
                    case TargetType.Self:
                        animationData.PlayAnimation(context.Caster);
                        break;
                    case TargetType.Enemy or TargetType.Ally:
                        animationData.PlayAnimation(oneTarget);
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
                    case TargetType.Enemy or TargetType.Ally:
                        CoroutineRunner.Instance.Run(vfxData.PlayVFX(oneTarget));
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

        context.Caster.ReceivedResource(ReceivedResourceValue);
        
        switch (TargetType)
        {
            case TargetType.Self:
                context.Caster.AffectResource(AffectedResource, AffectedResourceValue);
                break;
            case TargetType.Enemy or TargetType.Ally:
                oneTarget.AffectResource(AffectedResource, AffectedResourceValue);
                Debug.Log($"Ability {AbilityName} casted by {context.Caster.name} on {oneTarget.name}");
                break;
            case TargetType.Allies:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius, CustomAreaOfEffectPositioningDuration);
                if (Zone == Zone.CustomAreaOfEffect)
                {
                    yield return new WaitForSeconds(CustomAreaOfEffectPositioningDuration);
                }
                foreach (Unit ally in allies)
                {
                    if (AreaOfEffectRadius == 0 || Vector3.Distance(context.Caster.ZonePosition, ally.transform.position) <= AreaOfEffectRadius)
                    {
                        ally.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            case TargetType.Enemies:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius, CustomAreaOfEffectPositioningDuration);
                if (Zone == Zone.CustomAreaOfEffect)
                {
                    yield return new WaitForSeconds(CustomAreaOfEffectPositioningDuration);
                }
                foreach (Unit enemy in enemies)
                {
                    if (AreaOfEffectRadius == 0 || Vector3.Distance(context.Caster.ZonePosition, enemy.transform.position) <= AreaOfEffectRadius)
                    {
                        enemy.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            case TargetType.All:
                context.Caster.CreateZone(Zone, AreaOfEffectRadius, CustomAreaOfEffectPositioningDuration);
                if (Zone == Zone.CustomAreaOfEffect)
                {
                    yield return new WaitForSeconds(CustomAreaOfEffectPositioningDuration);
                }
                foreach (Unit ally in allies)
                {
                    if (AreaOfEffectRadius == 0 || Vector3.Distance(context.Caster.ZonePosition, ally.transform.position) <= AreaOfEffectRadius)
                    {
                        ally.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                foreach (Unit enemy in enemies)
                {
                    if (Vector3.Distance(context.Caster.ZonePosition, enemy.transform.position) <= AreaOfEffectRadius)
                    {
                        enemy.AffectResource(AffectedResource, AffectedResourceValue);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(TargetType), TargetType, null);
        }

        UIManager.Instance.StartAbilityCooldown(AbilityName);

        yield return new WaitForSeconds(CastTime);
        AbilitiesManager.Instance.AbilityFinishedAction(AbilityName, context.Caster.name);

        if (StatusEffectData != null)
        {
            switch (StatusEffectData.TargetType)
            {
                case TargetType.Self:
                    context.Caster.ApplyEffect(StatusEffectData);
                    break;
                case TargetType.Enemy or TargetType.Ally:
                    if (oneTarget == null)
                    {
                        if (TargetType == TargetType.Enemy)
                        {
                            oneTarget = enemies[Random.Range(0, enemies.Count)];
                        }
                        else if (TargetType == TargetType.Ally)
                        {
                            oneTarget = allies[Random.Range(0, allies.Count)];
                        }
                    }

                    oneTarget.ApplyEffect(StatusEffectData);
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

        Debug.Log($"Ability {AbilityName} casted by {context.Caster.name}");
    }
}