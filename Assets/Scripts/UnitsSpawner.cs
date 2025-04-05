using System.Collections.Generic;
using UnityEngine;

public class UnitsSpawner : Singleton<UnitsSpawner>
{
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private GameObject _spawnArea;
    private Vector3 _spawnAreaPosition;
    private MeshRenderer _spawnAreaMeshRenderer;
    private Vector3 _spawnAreaSize;
    private readonly Vector3 _offset = new Vector3(0.5f, 1f, 0.5f);

    [Header("First team")]
    [SerializeField] private int _firstTeamUnitsCount;
    [SerializeField] private Color _firstTeamColor;
    [SerializeField] private Transform _firstTeamParent;
    public List<Unit> FirstTeamUnits { get; private set; } = new List<Unit>();
    public const string FirstTeamUnitName = "FirstTeamUnit_";

    [Header("Second team")]
    [SerializeField] private int _secondTeamUnitsCount;
    [SerializeField] private Color _secondTeamColor;
    [SerializeField] private Transform _secondTeamParent;
    public List<Unit> SecondTeamUnits { get; private set; } = new List<Unit>();
    public const string SecondTeamUnitName = "SecondTeamUnit_";

    public Vector3 SpawnAreaSize => _spawnAreaSize;
    public List<Unit> AllUnits { get; private set; } = new List<Unit>();

    protected override void Awake()
    {
        base.Awake();
        _spawnAreaPosition = _spawnArea.transform.position;
        _spawnAreaMeshRenderer = _spawnArea.GetComponent<MeshRenderer>();
        _spawnAreaSize = _spawnAreaMeshRenderer.bounds.extents;
    }

    public void SpawnUnits()
    {
        for (int i = 0; i < _firstTeamUnitsCount; i++)
        {
            FirstTeamUnits.Add(SpawnUnit(_firstTeamColor, FirstTeamUnitName, _firstTeamParent));
        }

        for (int i = 0; i < _secondTeamUnitsCount; i++)
        {
            SecondTeamUnits.Add(SpawnUnit(_secondTeamColor, SecondTeamUnitName, _secondTeamParent));
        }
        AllUnits.AddRange(FirstTeamUnits);
        AllUnits.AddRange(SecondTeamUnits);
    }

    private Unit SpawnUnit(Color color, string team, Transform parent)
    {
        Unit unit = Instantiate(_unitPrefab, GetRandomPosition(), Quaternion.identity, parent);
        unit.SetColor(color);
        unit.SetUnitTeam(team);
        unit.gameObject.name = parent == _firstTeamParent
            ? FirstTeamUnitName + FirstTeamUnits.Count
            : SecondTeamUnitName + SecondTeamUnits.Count;

        return unit;
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 spawnPoint = Vector3.zero;
        float unitRadius = 1f;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(_spawnAreaPosition.x - _spawnAreaSize.x + _offset.x, _spawnAreaPosition.x + _spawnAreaSize.x - _offset.x);
            float z = Random.Range(_spawnAreaPosition.z - _spawnAreaSize.z + _offset.z, _spawnAreaPosition.z + _spawnAreaSize.z - _offset.z);
            spawnPoint = new Vector3(x, _offset.y, z);

            if (!Physics.CheckSphere(spawnPoint, unitRadius))
            {
                return spawnPoint;
            }
        }

        return spawnPoint;
    }
}