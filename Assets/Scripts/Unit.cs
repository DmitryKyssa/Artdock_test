using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Unit : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    public Action _selectAction;
    public Action _deselectAction;
    private readonly Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    [SerializeField] private float _moveDelay = 1f;
    [SerializeField] private float _moveDuration = 1f;
    private InputAction _moveSelectedAction;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _selectAction += OnSelectAction;
        _deselectAction += OnDeselectAction;

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
        _deselectAction?.Invoke();
    }

    private void OnDestroy()
    {
        _selectAction -= OnSelectAction;
        _deselectAction -= OnDeselectAction;
        _moveSelectedAction.Disable();
    }

    private void OnSelectAction()
    {
        StopAllCoroutines();

        _moveSelectedAction.performed += OnMoveSelected;
    }

    private void OnDeselectAction()
    {
        _moveSelectedAction.performed -= OnMoveSelected;
        StartCoroutine(MoveUnselected());
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
}