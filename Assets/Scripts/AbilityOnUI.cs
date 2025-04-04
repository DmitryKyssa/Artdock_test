using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityOnUI : MonoBehaviour
{
    public Image Icon;
    public TMP_Text SpentResourceText;
    public TMP_Text ReceivedResourceText;
    public TMP_Text KeyText;
    [HideInInspector] public string AbilityName;
    [HideInInspector] public float Cooldown;

    public void SetUpIcon()
    {
        Icon.type = Image.Type.Filled;
        Icon.fillMethod = Image.FillMethod.Radial360;
        Icon.fillOrigin = 0;
        Icon.fillAmount = 1;
        Icon.fillClockwise = true;
    }
}
