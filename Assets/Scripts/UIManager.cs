using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Button _startButton;

    public override void Awake()
    {
        base.Awake();
        _startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        UnitsSpawner.Instance.SpawnUnits();
        _startButton.interactable = false;
    }
}
