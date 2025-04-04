using UnityEngine;

public class AnimationData : ScriptableObject
{
    public AnimationClip AnimationClip;
    public TargetType TargetType;

    public void PlayAnimation(Unit unit)
    {
        unit.Animator.Play(AnimationClip.name);
        Debug.Log($"Playing animation: {AnimationClip.name} for target type: {TargetType}");
    }
}