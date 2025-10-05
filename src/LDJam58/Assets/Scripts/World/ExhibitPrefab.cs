
using UnityEngine;

public class ExhibitPrefab : MonoBehaviour
{
    [SerializeField] private WorldExhibitUI worldUi;

    private ExhibitTileType exhibitTileType;
    
    
    public ExhibitTileType ExhibitTileType => exhibitTileType;
    
    public void Init(ExhibitTileType exhibit)
    {
         worldUi.Init(exhibit);
         exhibitTileType = exhibit;
         //worldUi.gameObject.SetActive(true);
    }
    
}