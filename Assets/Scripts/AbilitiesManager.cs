using System.Collections.Generic;
using UnityEngine;

public class AbilitiesManager : Singleton<AbilitiesManager>
{
    [SerializeField] private bool _loadFromResources = true;
    [SerializeField] private List<AbilityData> _abilities = new List<AbilityData>();
    //TODO: Add keys

    protected override void Awake()
    {
        base.Awake();
        if (_loadFromResources)
        {
            LoadAbilities();
            SetAbilitiesForUI();
        }
    }

    private void LoadAbilities()
    {
        AbilityData[] abilities = Resources.LoadAll<AbilityData>("Abilities");
        foreach (AbilityData ability in abilities)
        {
            if (!_abilities.Contains(ability))
            {
                _abilities.Add(ability);
            }
        }

        Debug.Log($"Loaded {_abilities.Count} abilities from Resources.");
    }

    private void SetAbilitiesForUI()
    {
        for (int i = 0; i < _abilities.Count; i++)
        {
            AbilityOnUI abilityOnUI = UIManager.Instance.AbilitiesOnUI[i];
            abilityOnUI.Icon.sprite = _abilities[i].Sprite;
            abilityOnUI.SpentResourceText.text = $"{UIManager.StaminaText}{_abilities[i].ResourceCost}";
            abilityOnUI.ReceivedResourceText.text = $"{UIManager.XPText}{_abilities[i].ReceivedResourceValue}";
            abilityOnUI.AbilityName = _abilities[i].AbilityName;
            abilityOnUI.Cooldown = _abilities[i].CooldownAfterUsing;
            abilityOnUI.SetUpIcon();
        }
    }
}