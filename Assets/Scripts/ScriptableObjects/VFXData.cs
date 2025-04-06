using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "ScriptableObjects/VFXData", order = 1)]
public class VFXData : ScriptableObject
{
    public Material Material; //Yes, there need to be a VFX. I know, but I don't have time to make it now.
    public float EffectDuration;
    public TargetType TargetType;

    public IEnumerator PlayVFX(Unit unit)
    {
        Material material = unit.MeshRenderer.material;
        unit.MeshRenderer.sharedMaterial = Material;

        yield return new WaitForSeconds(EffectDuration);

        unit.MeshRenderer.sharedMaterial = material;
    }
}