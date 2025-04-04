using System.Collections;
using UnityEngine;

public class SFXData : ScriptableObject
{
    [ReadOnly] public AudioClip AudioClip;
    [ReadOnly] public bool IsOffsetFromStart;
    [ReadOnly] public float OffsetFromStart;

    public IEnumerator PlaySFX(Unit unit)
    {
        yield return new WaitForSeconds(OffsetFromStart);
        AudioSource.PlayClipAtPoint(AudioClip, unit.transform.position);
        Debug.Log($"Playing SFX: {AudioClip.name}");
    }
}