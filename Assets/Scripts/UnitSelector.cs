using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelector : Singleton<UnitSelector>
{
    [SerializeField] private List<LayerMask> _selectableLayers = new List<LayerMask>();
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private string _selectableTag;
    private InputAction _selectAction;
    private int _unitedLayerMask;

    public GameObject SelectedGO { get; private set; }

    public override void Awake()
    {
        base.Awake();
        _selectAction = new InputAction("Select", InputActionType.Button, "<Mouse>/leftButton");
        _selectAction.Enable();
        _selectAction.performed += OnSelect;

        foreach (LayerMask layer in _selectableLayers)
        {
            _unitedLayerMask |= layer.value;
        }
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _unitedLayerMask))
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
                    SelectedGO.GetComponent<Unit>()._deselectAction?.Invoke();
                }

                SelectedGO = hit.collider.gameObject;
                SelectedGO.transform.GetChild(0).gameObject.SetActive(true);
                SelectedGO.GetComponent<Unit>()._selectAction?.Invoke();
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