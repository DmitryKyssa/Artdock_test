﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Unit : MonoBehaviour, IEffectable
{
    private string _unitTeam;
    public string UnitTeam => _unitTeam;

    private MeshRenderer _meshRenderer;
    private Animator _animator;
    [SerializeField] private Transform _vfxCastPoint;

    public Action selectAction;
    public Action deselectAction;
    private readonly Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    [SerializeField] private float _moveDelay = 1f;
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private int _XP;
    private int _HP;
    public const int MaxHP = 100;
    private int _stamina;
    public const int MaxStamina = 100;
    [SerializeField] private int _restoreStaminaValue = 1;
    [SerializeField] private int _restoreStaminaDelay = 1;
    private InputAction _moveSelectedAction;
    private Coroutine _unselectedMoveCoroutine;
    private Coroutine _restoreStaminaCoroutine;
    private bool _isSelected = false;

    [SerializeField] private GameObject _abilityZoneGO;
    private InputAction _abilityZoneAction;

    private float _castDistance = 1.5f;
    private float _castRadius = 0.5f;

    private Dictionary<StatusEffectData, Coroutine> _activeStatusEffects = new Dictionary<StatusEffectData, Coroutine>();

    public Animator Animator => _animator;
    public Transform VfxCastPoint => _vfxCastPoint;
    public int Stamina => _stamina;
    public Vector3 ZonePosition => _abilityZoneGO.transform.position;
    public MeshRenderer MeshRenderer => _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _animator = GetComponent<Animator>();

        selectAction += OnSelectAction;
        deselectAction += OnDeselectAction;

        _moveSelectedAction = new InputAction("MoveSelected", InputActionType.Value);
        _moveSelectedAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveSelectedAction.Enable();

        _abilityZoneAction = new InputAction("AbilityZone", InputActionType.Value);
        _abilityZoneAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        _abilityZoneAction.Disable();
    }

    private void Start()
    {
        deselectAction?.Invoke();
        _HP = MaxHP;
        _stamina = MaxStamina;
        _abilityZoneGO.SetActive(false);
    }

    private void OnDestroy()
    {
        selectAction -= OnSelectAction;
        deselectAction -= OnDeselectAction;
        _moveSelectedAction.Disable();
    }

    private void OnSelectAction()
    {
        if (_unselectedMoveCoroutine != null)
        {
            StopCoroutine(_unselectedMoveCoroutine);
            _unselectedMoveCoroutine = null;
        }
        _isSelected = true;
        UIManager.Instance.DeactivateUnitProperties();
        UIManager.Instance.ActivateUnitProperties(_XP, _HP, MaxHP, _stamina, MaxStamina);
        _moveSelectedAction.performed += OnMoveSelected;
    }

    private void OnDeselectAction()
    {
        _moveSelectedAction.performed -= OnMoveSelected;
        _unselectedMoveCoroutine = StartCoroutine(MoveUnselected());
        UIManager.Instance.DeactivateUnitProperties();
        _isSelected = false;
    }

    private void OnMoveSelected(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        Vector3 targetPosition = transform.position + new Vector3(direction.x, 0f, direction.y);
        float clampedX = Mathf.Clamp(targetPosition.x, -UnitsSpawner.Instance.SpawnAreaSize.x, UnitsSpawner.Instance.SpawnAreaSize.x);
        float clampedZ = Mathf.Clamp(targetPosition.z, -UnitsSpawner.Instance.SpawnAreaSize.z, UnitsSpawner.Instance.SpawnAreaSize.z);
        targetPosition = new Vector3(clampedX, transform.position.y, clampedZ);
        transform.position = targetPosition;
    }

    private IEnumerator MoveUnselected()
    {
        while (true)
        {
            Vector3[] shuffledDirections = _directions.OrderBy(_ => UnityEngine.Random.value).ToArray();
            Vector3? chosenDirection = null;

            foreach (var dir in shuffledDirections)
            {
                if (Physics.SphereCast(transform.position, _castRadius, dir, out RaycastHit hit, _castDistance))
                {
                    if (hit.collider.CompareTag(UnitSelector.Instance.SelectableTag))
                    {
                        continue;
                    }
                }

                chosenDirection = dir;
                break;
            }

            if (chosenDirection == null)
            {
                yield return new WaitForSeconds(_moveDelay);
                continue;
            }

            Vector3 direction = chosenDirection.Value;
            float clampedX = Mathf.Clamp(transform.position.x + direction.x, -UnitsSpawner.Instance.SpawnAreaSize.x, UnitsSpawner.Instance.SpawnAreaSize.x);
            float clampedZ = Mathf.Clamp(transform.position.z + direction.z, -UnitsSpawner.Instance.SpawnAreaSize.z, UnitsSpawner.Instance.SpawnAreaSize.z);

            Vector3 targetPosition = new Vector3(clampedX, transform.position.y, clampedZ);
            Vector3 startPosition = transform.position;
            float startTime = Time.time;

            while (true)
            {
                float t = Mathf.Clamp01((Time.time - startTime) / _moveDuration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                if (t >= 1f)
                {
                    break;
                }

                yield return null;
            }


            transform.position = targetPosition;
            yield return new WaitForSeconds(_moveDelay);
        }
    }

    public void SetColor(Color color)
    {
        _meshRenderer.material.color = color;
    }

    public void SetUnitTeam(string team)
    {
        _unitTeam = team;
    }

    public void AddXP(int value)
    {
        _XP += value;
        if (_isSelected)
        {
            UIManager.Instance.UpdateXPText(_XP);
        }
    }

    private IEnumerator RestoreStaminaPeriodically()
    {
        WaitForSeconds wait = new WaitForSeconds(_restoreStaminaDelay);
        while (_stamina < MaxStamina)
        {
            _stamina = Mathf.Clamp(_stamina + _restoreStaminaValue, 0, MaxStamina);
            if (_isSelected)
            {
                UIManager.Instance.UpdateStaminaText(_stamina);
            }
            yield return wait;
        }

        _restoreStaminaCoroutine = null;
    }

    public void SpendStamina(int value)
    {
        _stamina = Mathf.Clamp(_stamina - value, 0, MaxStamina);
        if (_isSelected)
        {
            UIManager.Instance.UpdateStaminaText(_stamina);
        }

        _restoreStaminaCoroutine ??= StartCoroutine(RestoreStaminaPeriodically());
    }

    public void DeductResource(int value)
    {
        SpendStamina(value);
    }

    public void AffectResource(AffectedResourceType affectedResource, float value)
    {
        switch (affectedResource)
        {
            case AffectedResourceType.HP:
                ChangeHP((int)value);
                break;
            case AffectedResourceType.MovementDuration:
                _moveDuration += value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(affectedResource), affectedResource, null);
        }
    }

    public void ReceivedResource(int value)
    {
        AddXP(value);
    }

    public void CreateZone(Zone zone, float area, float duration)
    {
        switch (zone)
        {
            case Zone.SingleTarget:
                _abilityZoneGO.SetActive(false);
                break;
            case Zone.AutoAreaOfEffect:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.localScale = new Vector3(area, area, area);
                StartCoroutine(DisableZoneAfterDuration(duration));
                break;
            case Zone.CustomAreaOfEffect:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.localScale = new Vector3(area, area, area);
                _abilityZoneAction.Enable();
                StartCoroutine(DisableZoneAfterDuration(duration));
                UIManager.Instance.ShowArrowsImage(true);
                _abilityZoneAction.performed += OnZoneMove;
                break;
            case Zone.AllLocation:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.position = new Vector3(0f, 0f, 0f);
                _abilityZoneGO.transform.localScale = new Vector3(1f, 1f, 1f);
                StartCoroutine(DisableZoneAfterDuration(duration));
                break;
        }
    }

    private void OnZoneMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        Vector3 targetPosition = transform.position + new Vector3(direction.x, 0f, direction.y);
        _abilityZoneGO.transform.position = targetPosition;
    }

    private IEnumerator DisableZoneAfterDuration(float duration)
    {
        if (duration == 0f)
        {
            duration = 1f;
        }

        yield return new WaitForSeconds(duration);
        _abilityZoneGO.SetActive(false);
        if (_abilityZoneAction.enabled)
        {
            _abilityZoneAction.performed -= OnZoneMove;
            _abilityZoneAction.Disable();
            UIManager.Instance.ShowArrowsImage(false);
        }
    }

    private void ChangeHP(int value)
    {
        _HP = Mathf.Clamp(_HP + value, 0, MaxHP);
        Debug.Log($"Unit {gameObject.name} HP changed to {_HP}");
        if (_isSelected)
        {
            UIManager.Instance.UpdateHPText(_HP);
        }

        if (_HP == 0)
        {
            Debug.Log($"Unit {gameObject.name} is dead.");
            AbilitiesManager.Instance.RemoveUnitFromDictionary(name);
            Destroy(gameObject);
        }
    }

    public void ApplyEffect(StatusEffectData statusEffect)
    {
        if (_activeStatusEffects.ContainsKey(statusEffect))
        {
            if (statusEffect.IsEndless)
            {
                Debug.Log($"Endless effect {statusEffect.StatusEffectName} is already applied. Skipping.");
                AddXP(10);
                return;
            }

            Debug.Log($"Resetting effect: {statusEffect.StatusEffectName}");
            StopCoroutine(_activeStatusEffects[statusEffect]);
            _activeStatusEffects.Remove(statusEffect);
        }

        Coroutine routine;
        if (statusEffect.IsEndless)
        {
            routine = StartCoroutine(ApplyEndlessEffect(statusEffect));
            Debug.Log($"Applying endless effect: {statusEffect.StatusEffectName}");
        }
        else
        {
            routine = StartCoroutine(ApplyTimedEffect(statusEffect));
            Debug.Log($"Applying timed effect: {statusEffect.StatusEffectName} for {statusEffect.Duration} seconds");
        }

        _activeStatusEffects[statusEffect] = routine;
    }

    private IEnumerator ApplyEndlessEffect(StatusEffectData statusEffect)
    {
        WaitForSeconds wait = new WaitForSeconds(statusEffect.Period);
        while (true)
        {
            AffectResource(statusEffect.AffectedResource, statusEffect.AffectedResourceValuePerPeriod);
            yield return wait;
        }
    }

    private IEnumerator ApplyTimedEffect(StatusEffectData statusEffect)
    {
        WaitForSeconds wait = new WaitForSeconds(statusEffect.Period);
        if (statusEffect.IsPeriodic)
        {
            for (int i = 0; i < statusEffect.Duration / statusEffect.Period; i++)
            {
                AffectResource(statusEffect.AffectedResource, statusEffect.AffectedResourceValuePerPeriod);
                yield return wait;
            }
        }
        else
        {
            yield return new WaitForSeconds(statusEffect.Duration);
            AffectResource(statusEffect.AffectedResource, statusEffect.AffectedResourceValuePerPeriod);
        }

        ((IEffectable)this).RemoveEffect(statusEffect);
    }

    void IEffectable.RemoveEffect(StatusEffectData statusEffect)
    {
        if (_activeStatusEffects.TryGetValue(statusEffect, out Coroutine routine))
        {
            StopCoroutine(routine);
            _activeStatusEffects.Remove(statusEffect);
            Debug.Log($"Removed effect: {statusEffect.StatusEffectName} from {gameObject.name}");
        }
    }
}