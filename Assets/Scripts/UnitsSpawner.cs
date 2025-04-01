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
    [SerializeField] private LayerMask _firstTeamLayer;
    [SerializeField] private Transform _firstTeamParent;

    [Header("Second team")]
    [SerializeField] private int _secondTeamUnitsCount;
    [SerializeField] private Color _secondTeamColor;
    [SerializeField] private LayerMask _secondTeamLayer;
    [SerializeField] private Transform _secondTeamParent;

    public override void Awake()
    {
        base.Awake();
        _spawnAreaPosition = _spawnArea.transform.position;
        _spawnAreaMeshRenderer = _spawnArea.GetComponent<MeshRenderer>();
        _spawnAreaSize = _spawnAreaMeshRenderer.bounds.extents;
    }

    public void SpawnUnits()
    {
        int layer = (int)Mathf.Log(_firstTeamLayer.value, 2);
        for (int i = 0; i < _firstTeamUnitsCount; i++)
        {
            SpawnUnit(_firstTeamColor, layer, _firstTeamParent);
        }

        layer = (int)Mathf.Log(_secondTeamLayer.value, 2);
        for (int i = 0; i < _secondTeamUnitsCount; i++)
        {
            SpawnUnit(_secondTeamColor, layer, _secondTeamParent);
        }
    }

    private void SpawnUnit(Color color, int layer, Transform parent)
    {
        Unit unit = Instantiate(_unitPrefab, GetRandomPosition(), Quaternion.identity, parent);
        unit.SetColor(color);
        unit.SetLayer(layer);
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