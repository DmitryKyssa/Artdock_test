using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitiesManager : Singleton<AbilitiesManager>
{
    private List<AbilityData> _abilities = new List<AbilityData>();
    private List<InputAction> _actions = new List<InputAction>();
    private Dictionary<string, Dictionary<string, bool>> _activeAbilitiesPerUnit = new();

    public Action<string, string> AbilityFinishedAction; //first string - ability name, second string - unitName name

    protected override void Awake()
    {
        base.Awake();

        LoadAbilities();
        SetAbilitiesForUI();

        AbilityFinishedAction += (abilityName, unit) =>
        {
            if (_activeAbilitiesPerUnit.TryGetValue(unit, out var unitAbilities) &&
                unitAbilities.ContainsKey(abilityName))
            {
                unitAbilities[abilityName] = false;
            }
        };

        UnitsSpawner.Instance.OnSpawnedAction += () =>
        {
            foreach (var unit in UnitsSpawner.Instance.AllUnits)
            {
                if (!_activeAbilitiesPerUnit.ContainsKey(unit.name))
                {
                    _activeAbilitiesPerUnit[unit.name] = new Dictionary<string, bool>();
                }

                foreach (var ability in _abilities)
                {
                    _activeAbilitiesPerUnit[unit.name][ability.AbilityName] = false;
                }
            }
            Debug.Log($"Initialized {_abilities.Count} abilities for {_activeAbilitiesPerUnit.Count} units.");
        };
    }

    public void RemoveUnitFromDictionary(string unitName)
    {
        if (_activeAbilitiesPerUnit.ContainsKey(unitName))
        {
            _activeAbilitiesPerUnit.Remove(unitName);
        }
    }

    private void LoadAbilities()
    {
        AbilityData[] loadedAbilities = Resources.LoadAll<AbilityData>("Abilities");

        foreach (var ability in loadedAbilities)
        {
            if (_abilities.Contains(ability))
            {
                continue;
            }

            _abilities.Add(ability);

            InputAction action = new InputAction(ability.AbilityName, InputActionType.Button, "<Keyboard>/" + (_abilities.Count));
            action.Enable();

            action.performed += context =>
            {
                Unit selectedUnit = UnitSelector.Instance.SelectedGO;

                if (_activeAbilitiesPerUnit.TryGetValue(selectedUnit.name, out var unitAbilities))
                {
                    if (unitAbilities[ability.AbilityName])
                    {
                        Debug.Log($"Ability {ability.AbilityName} is already active for unit {selectedUnit.name}");
                        return;
                    }

                    if (selectedUnit.Stamina < ability.ResourceCost)
                    {
                        return;
                    }

                    AbilityContext abilityContext = new AbilityContext
                    {
                        Caster = selectedUnit,
                        CastPoint = selectedUnit.VfxCastPoint.position,
                    };

                    Debug.Log($"Casting ability {ability.AbilityName} from {selectedUnit.name}");
                    StartCoroutine(ability.CastAbility(abilityContext));

                    unitAbilities[ability.AbilityName] = true;
                }
            };

            _actions.Add(action);
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
            abilityOnUI.KeyText.text = $"'{i + 1}'";
        }
    }

    private void OnDestroy()
    {
        foreach (var action in _actions)
        {
            action.Disable();
            action.Dispose();
        }

        foreach (var unitAbilities in _activeAbilitiesPerUnit.Values)
        {
            unitAbilities.Clear();
        }

        _activeAbilitiesPerUnit.Clear();
    }
}