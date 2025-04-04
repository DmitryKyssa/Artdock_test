using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitiesManager : Singleton<AbilitiesManager>
{
    [SerializeField] private bool _loadFromResources = true;
    [SerializeField] private List<AbilityData> _abilities = new List<AbilityData>();
    private List<InputAction> _actions = new List<InputAction>();
    private Dictionary<string, bool> _activeAbilities = new Dictionary<string, bool>();
    public Action<string> AbilityFinishedAction;

    protected override void Awake()
    {
        base.Awake();
        if (_loadFromResources)
        {
            LoadAbilities();
            SetAbilitiesForUI();
        }

        AbilityFinishedAction += abilityName =>
        {
            if (_activeAbilities.ContainsKey(abilityName))
            {
                _activeAbilities[abilityName] = false;
            }
        };
    }

    private void LoadAbilities()
    {
        AbilityData[] abilities = Resources.LoadAll<AbilityData>("Abilities");
        for (int i = 0; i < abilities.Length; i++)
        {
            int index = i;
            if (!_abilities.Contains(abilities[i]))
            {
                _abilities.Add(abilities[i]);
                _activeAbilities.Add(abilities[i].AbilityName, false);
                InputAction action = new InputAction(_abilities[^1].AbilityName, InputActionType.Button, "<Keyboard>/" + (i + 1));
                if (_abilities[^1].Condition == Condition.None)
                {
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }

                action.performed += context =>
                {
                    if (_activeAbilities[_abilities[index].AbilityName])
                    {
                        return;
                    }

                    AbilityContext abilityContext = new AbilityContext
                    {
                        Caster = UnitSelector.Instance.SelectedGO.GetComponent<Unit>(),
                        CastPoint = UnitSelector.Instance.SelectedGO.GetComponent<Unit>().VfxCastPoint.position,
                    };

                    if (abilityContext.Caster.Stamina < _abilities[index].ResourceCost)
                    {
                        return;
                    }

                    Debug.Log($"Ability {_abilities[index].AbilityName} casted from {abilityContext.Caster.gameObject.name}");
                    StartCoroutine(_abilities[index].CastAbility(abilityContext));
                    _activeAbilities[_abilities[index].AbilityName] = true;
                };

                _actions.Add(action);
            }
        }
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
            abilityOnUI.KeyText.text = $"'{i + 1}'";
        }
    }
}