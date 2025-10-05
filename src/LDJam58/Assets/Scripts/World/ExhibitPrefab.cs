
using UnityEngine;

public class ExhibitPrefab : MonoBehaviour
{
    [SerializeField] private WorldExhibitUI worldUi;

    public void Init(ExhibitTileType exhibit)
    {
         worldUi.Init(exhibit);
         worldUi.gameObject.SetActive(true);
    }
}