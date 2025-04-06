using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "ScriptableObjects/VFXData", order = 1)]
public class VFXData : ScriptableObject
{
    public ParticleSystem ParticleSystem;
    public bool IsOffsetFromStart;
    public float OffsetFromStart;
    public TargetType TargetType;

    public IEnumerator PlayVFX(Unit unit)
    {
        yield return new WaitForSeconds(OffsetFromStart);
        GameObject vfxInstance = Instantiate(ParticleSystem.gameObject, unit.VfxCastPoint.position, Quaternion.identity);
        Destroy(vfxInstance, ParticleSystem.main.duration);
        Debug.Log($"Playing VFX: {ParticleSystem.name}");
    }
}