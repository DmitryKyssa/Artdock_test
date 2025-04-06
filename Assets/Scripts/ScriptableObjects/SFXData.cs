using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SFXData", menuName = "ScriptableObjects/SFXData", order = 1)]
public class SFXData : ScriptableObject
{
    public AudioClip AudioClip;
    public bool IsOffsetFromStart;
    public float OffsetFromStart;

    public IEnumerator PlaySFX(Unit unit)
    {
        yield return new WaitForSeconds(OffsetFromStart);
        AudioSource.PlayClipAtPoint(AudioClip, unit.transform.position);
        Debug.Log($"Playing SFX: {AudioClip.name}");
    }
}