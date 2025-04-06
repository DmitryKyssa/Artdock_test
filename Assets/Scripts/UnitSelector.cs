using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelector : Singleton<UnitSelector>
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private string _selectableTag;
    private InputAction _selectAction;

    public string SelectableTag => _selectableTag;
    public Unit SelectedGO { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _selectAction = new InputAction("Select", InputActionType.Button, "<Mouse>/leftButton");
        _selectAction.Enable();
        _selectAction.performed += OnSelect;
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag(_selectableTag))
            {
                if (SelectedGO != null)
                {
                    if (SelectedGO == hit.collider.gameObject)
                    {
                        return;
                    }

                    SelectedGO.transform.GetChild(0).gameObject.SetActive(false);
                    SelectedGO.GetComponent<Unit>().deselectAction?.Invoke();
                }

                SelectedGO = hit.collider.gameObject.GetComponent<Unit>();
                SelectedGO.transform.GetChild(0).gameObject.SetActive(true);
                SelectedGO.selectAction?.Invoke();
            }
        }
    }

    private void OnDisable()
    {
        _selectAction.Disable();
        _selectAction.performed -= OnSelect;
        SelectedGO = null;
    }
}