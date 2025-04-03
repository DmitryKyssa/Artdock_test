using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Button _startButton;

    [Header("Unit properties")]
    [SerializeField] private GameObject _unitPropertiesPanel;
    [SerializeField] private TMP_Text _xpText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _staminaText;
    [SerializeField] private TMP_Text _damageText;
    private const string XPText = "XP: ";
    private const string HPText = "HP: ";
    private const string StaminaText = "Stamina: ";
    private const string DamageText = "Damage: ";

    public override void Awake()
    {
        base.Awake();
        _startButton.onClick.AddListener(StartGame);
        _unitPropertiesPanel.SetActive(false);
    }

    private void StartGame()
    {
        UnitsSpawner.Instance.SpawnUnits();
        _startButton.interactable = false;
    }

    public void ActivateUnitProperties(int xp, int hp, int maxHP, int stamina, int maxStamina, int damage)
    {
        _unitPropertiesPanel.SetActive(true);
        _xpText.text = XPText + xp;
        _hpText.text = HPText + hp + "/" + maxHP;
        _staminaText.text = StaminaText + stamina + "/" + maxStamina;
        _damageText.text = DamageText + damage;
    }

    public void DeactivateUnitProperties()
    {
        _unitPropertiesPanel.SetActive(false);
    }

    public void UpdateHPText(int hp, int maxHP)
    {
        _hpText.text = HPText + hp + "/" + maxHP;
    }

    public void UpdateStaminaText(int stamina, int maxStamina)
    {
        _staminaText.text = StaminaText + stamina + "/" + maxStamina;
    }

    public void UpdateXPText(int xp)
    {
        _xpText.text = XPText + xp;
    }

    public void UpdateDamageText(int damage)
    {
        _damageText.text = DamageText + damage;
    }
}