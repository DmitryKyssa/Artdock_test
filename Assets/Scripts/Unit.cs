using System;
using System.Collections;
using System.Linq;
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
    private InputAction _abilityZoneAction;

    private float _castDistance = 1.5f;
    private float _castRadius = 0.5f;

    public Animator Animator => _animator;
    public Transform VfxCastPoint => _vfxCastPoint;
    public int Stamina => _stamina;
    public Vector3 ZonePosition => _abilityZoneGO.transform.position;

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
            float elapsedTime = 0f;

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
                _abilityZoneAction.performed += OnZoneMove;
                break;
            case Zone.AllLocation:
                _abilityZoneGO.SetActive(true);
                _abilityZoneGO.transform.position = new Vector3(0f, 0f, 0f);
                _abilityZoneGO.transform.localScale = new Vector3(UnitsSpawner.Instance.SpawnAreaSize.x, 1f, UnitsSpawner.Instance.SpawnAreaSize.z);
                StartCoroutine(DisableZoneAfterDuration(duration));
                break;
        }
    }

    private void OnZoneMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        Vector3 targetPosition = transform.position + new Vector3(direction.x, 0f, direction.y);
        float clampedX = Mathf.Clamp(targetPosition.x, -UnitsSpawner.Instance.SpawnAreaSize.x, UnitsSpawner.Instance.SpawnAreaSize.x);
        float clampedZ = Mathf.Clamp(targetPosition.z, -UnitsSpawner.Instance.SpawnAreaSize.z, UnitsSpawner.Instance.SpawnAreaSize.z);
        targetPosition = new Vector3(clampedX, transform.position.y, clampedZ);
        _abilityZoneGO.transform.position = targetPosition;
    }

    private IEnumerator DisableZoneAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        _abilityZoneGO.SetActive(false);
        if (_abilityZoneAction.enabled)
        {
            _abilityZoneAction.performed -= OnZoneMove;
            _abilityZoneAction.Disable();
        }
    }

    private void ChangeHP(int value)
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