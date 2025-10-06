using System.Collections.Generic;
using UnityEngine;

public class GhostTile2 : MonoBehaviour
{
    [SerializeField] private Material errorMaterial;
    [SerializeField] private Material ghostMaterial;

    private Material _material;
    private GameObject _ghostPlaceable;
    private Vector3 _snapTo;
    private List<Renderer> _ghostRenderers = new List<Renderer>();

    public void Init(ExhibitTileType exhibitTileType)
    {
        transform.rotation = Quaternion.identity;
        _ghostPlaceable = Instantiate(exhibitTileType.ExhibitPrefab, transform);
        _ghostPlaceable.GetComponent<ExhibitPrefab>().Init(exhibitTileType);
        SetLayerRecursively(_ghostPlaceable, LayerMask.NameToLayer("Ghost"));
        CollectRenderers();
        SetCollidersToTrigger();
        UpdateMaterial(errorMaterial);
    }

    public void CleanUp()
    {
        transform.rotation = Quaternion.identity;
        Destroy(_ghostPlaceable);
        _ghostPlaceable = null;
        _ghostRenderers = new List<Renderer>();
    }
        
    public void OutOfBounds()
    {
        UpdateMaterial(errorMaterial);
    }

    public void Invalid()
    {
        UpdateMaterial(errorMaterial);
    }
    
    public void Valid()
    {
        UpdateMaterial(ghostMaterial);
    }
    
    private void SetCollidersToTrigger()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>(true))
            if (col is MeshCollider meshCollider)
                if (meshCollider.convex)
                    col.isTrigger = true;
    }
    
    private void CollectRenderers()
    {
        var renderers = new List<Renderer>();
        var mainRend = _ghostPlaceable.GetComponent<Renderer>();
        if (mainRend != null)
            renderers.Add(mainRend);
        renderers.AddRange(_ghostPlaceable.GetComponentsInChildren<Renderer>());
        _ghostRenderers = renderers;
    }
    
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
    
    private void UpdateMaterial(Material material)
    {
        if (material == _material)
            return;
        _material = material;
        foreach (var renderer in _ghostRenderers)
            renderer.material = _material;
    }
        
}