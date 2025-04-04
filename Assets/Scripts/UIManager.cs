using System.Collections;
using System.Collections.Generic;
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
    public const string XPText = "XP: ";
    public const string HPText = "HP: ";
    public const string StaminaText = "Stamina: ";

    [Header("Abilities")]
    [SerializeField] private GameObject _abilitiesPanel;
    [SerializeField] private List<AbilityOnUI> _abilitiesOnUI = new List<AbilityOnUI>();
    [SerializeField] private GameObject _keyboardImageGO;

    public List<AbilityOnUI> AbilitiesOnUI { get => _abilitiesOnUI; set => _abilitiesOnUI = value; }

    protected override void Awake()
    {
        base.Awake();
        _startButton.onClick.AddListener(StartGame);
        _unitPropertiesPanel.SetActive(false);
        _abilitiesPanel.SetActive(false);
        _keyboardImageGO.SetActive(false);
    }

    private void StartGame()
    {
        UnitsSpawner.Instance.SpawnUnits();
        _startButton.interactable = false;
    }

    public void ActivateUnitProperties(int xp, int hp, int maxHP, int stamina, int maxStamina)
    {
        _unitPropertiesPanel.SetActive(true);
        _abilitiesPanel.SetActive(true);
        _keyboardImageGO.SetActive(true);
        _xpText.text = XPText + xp;
        _hpText.text = HPText + hp + "/" + maxHP;
        _staminaText.text = StaminaText + stamina + "/" + maxStamina;
    }

    public void DeactivateUnitProperties()
    {
        _unitPropertiesPanel.SetActive(false);
        _abilitiesPanel.SetActive(false);
        _keyboardImageGO.SetActive(false);
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

    public void StartAbilityCooldown(string abilityName)
    {
        StartCoroutine(AbilityCooldown(abilityName));
    }

    private IEnumerator AbilityCooldown(string abilityName)
    {
        for(int i = 0; i < _abilitiesOnUI.Count; i++)
        {
            if (_abilitiesOnUI[i].AbilityName == abilityName)
            {
                _abilitiesOnUI[i].Icon.fillAmount = 1;
                while (_abilitiesOnUI[i].Icon.fillAmount > 0)
                {
                    _abilitiesOnUI[i].Icon.fillAmount -= Time.deltaTime / _abilitiesOnUI[i].Cooldown;
                    yield return null;
                }
                break;
            }
        }
    }
}