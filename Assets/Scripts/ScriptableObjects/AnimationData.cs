using UnityEngine;

[CreateAssetMenu(fileName = "AnimationData", menuName = "ScriptableObjects/AnimationData", order = 1)]
public class AnimationData : ScriptableObject //no implementation yet
{
    public AnimationClip AnimationClip;
    public TargetType TargetType;

    public void PlayAnimation(Unit unit)
    {
        unit.Animator.Play(AnimationClip.name);
        Debug.Log($"Playing animation: {AnimationClip.name} for target type: {TargetType}");
    }
}