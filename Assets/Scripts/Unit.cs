using UnityEngine;

public class Unit: MonoBehaviour 
{
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
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