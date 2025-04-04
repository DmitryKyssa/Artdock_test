using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Unit : MonoBehaviour, IEffectable
{
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
    private Coroutine _restoreStaminaCoroutine;
    [SerializeField] private GameObject _abilityZoneGO;

    public Animator Animator => _animator;
    public Transform VfxCastPoint => _vfxCastPoint;

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
    }

    private void Start()
    {
        deselectAction?.Invoke();
        _HP = MaxHP - 20;
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
        StopAllCoroutines();
        UIManager.Instance.ActivateUnitProperties(_XP, _HP, MaxHP, _stamina, MaxStamina);
        _moveSelectedAction.performed += OnMoveSelected;
    }

    private void OnDeselectAction()
    {
        _moveSelectedAction.performed -= OnMoveSelected;
        StartCoroutine(MoveUnselected());
        UIManager.Instance.DeactivateUnitProperties();
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
            Vector3 randomDirection = _directions[UnityEngine.Random.Range(0, _directions.Length)];
            float clampedX = Mathf.Clamp(transform.position.x + randomDirection.x, -UnitsSpawner.Instance.SpawnAreaSize.x, UnitsSpawner.Instance.SpawnAreaSize.x);
            float clampedZ = Mathf.Clamp(transform.position.z + randomDirection.z, -UnitsSpawner.Instance.SpawnAreaSize.z, UnitsSpawner.Instance.SpawnAreaSize.z);
            Vector3 targetPosition = new Vector3(clampedX, transform.position.y, clampedZ);
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;
            while (elapsedTime < _moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / _moveDuration);
                elapsedTime += Time.deltaTime;
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

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
    }

    public void AddXP(int value)
    {
        _XP += value;
        UIManager.Instance.UpdateXPText(_XP);
    }

    public void RestoreStamina(int value)
    {
        _stamina = Mathf.Clamp(_stamina + value, 0, MaxStamina);
    }

    private IEnumerator RestoreStaminaPeriodically()
    {
        while (_stamina < MaxStamina)
        {
            RestoreStamina(_restoreStaminaValue);
            UIManager.Instance.UpdateStaminaText(_stamina);
            yield return new WaitForSeconds(_restoreStaminaDelay);
        }

        _restoreStaminaCoroutine = null;
    }

    public void SpendStamina(int value)
    {
        _stamina = Mathf.Clamp(_stamina - value, 0, MaxStamina);
        Debug.Log($"Stamina: {_stamina}/{MaxStamina} for GO {gameObject.name}");
        UIManager.Instance.UpdateStaminaText(_stamina);

        _restoreStaminaCoroutine ??= StartCoroutine(RestoreStaminaPeriodically());
    }

    public void DeductResource(int value)
    {
        SpendStamina(value);
    }

    public void AffectResource(AffectedResourceType affectedResource, int value)
    {
        switch (affectedResource)
        {
            case AffectedResourceType.HP:
                ChangeHP(value);
                break;
            case AffectedResourceType.MovementSpeed:
                _moveDuration = Mathf.Infinity;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(affectedResource), affectedResource, null);
        }
    }

    public void RecievedResource(int value)
    {
        AddXP(value);
    }

    public void DestroyZone()
    {
        _abilityZoneGO.SetActive(false);
    }

    public void CreateZone(Zone zone, float area)
    {
        switch (zone)
        {
            case Zone.SingleTarget:
                _abilityZoneGO.SetActive(false);
                break;
            case Zone.AutoAreaOfEffect:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.localScale = new Vector3(area, area, area);
                break;
            case Zone.CustomAreaOfEffect:
                break;
            case Zone.AllLocation:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.localScale = new Vector3(UnitsSpawner.Instance.SpawnAreaSize.x, 1f, UnitsSpawner.Instance.SpawnAreaSize.z);
                break;
        }
    }

    public void ChangeHP(int value)
    {
        _HP = Mathf.Clamp(_HP + value, 0, MaxHP);
        UIManager.Instance.UpdateHPText(_HP);

        if (_HP == 0)
        {
            Debug.Log($"Unit {gameObject.name} is dead.");
            Destroy(gameObject);
        }
    }

    public void ApplyEffect(StatusEffectData statusEffect)
    {
        if (statusEffect.IsEndless)
        {
            StartCoroutine(ApplyEndlessEffect(statusEffect));
            Debug.Log($"Applying endless effect: {statusEffect.StatusEffectName}");
        }
        else
        {
            Debug.Log($"Applying timed effect: {statusEffect.StatusEffectName} for {statusEffect.Duration} seconds");
            StartCoroutine(ApplyTimedEffect(statusEffect));
        }
    }

    private IEnumerator ApplyEndlessEffect(StatusEffectData statusEffect)
    {
        while (true)
        {
            AffectResource(statusEffect.AffectedResource, statusEffect.AffectedResourceValuePerPeriod);
            yield return new WaitForSeconds(statusEffect.Period);
        }
    }

    private IEnumerator ApplyTimedEffect(StatusEffectData statusEffect)
    {
        if (statusEffect.IsPeriodic)
        {
            for (int i = 0; i < statusEffect.Duration / statusEffect.Period; i++)
            {
                AffectResource(statusEffect.AffectedResource, statusEffect.AffectedResourceValuePerPeriod);
                yield return new WaitForSeconds(statusEffect.Period);
            }
        }

        RemoveEffect(statusEffect);
    }

    public void RemoveEffect(StatusEffectData statusEffect)
    {
        Debug.Log($"Removing effect: {statusEffect.StatusEffectName} from {gameObject.name}");
    }
}