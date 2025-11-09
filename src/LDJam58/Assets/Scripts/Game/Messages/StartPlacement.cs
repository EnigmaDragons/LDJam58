

using UnityEngine;

public class StartPlacement
{
    public ExhibitTileType exhibit;
    public Vector3 UiNumberPosition;
    
    public StartPlacement(ExhibitTileType exhibit, Vector3 uiNumberPosition)
    {
        this.exhibit = exhibit;
        UiNumberPosition = uiNumberPosition;
    }
}
