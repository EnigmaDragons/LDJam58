using UnityEngine;

public class ExhibitPlaced
{
    public GameObject exhibitInstance;
    public ExhibitTileType exhibitTileType;

    public ExhibitPlaced(GameObject exhibitInstance, ExhibitTileType exhibitTileType)
    {
        this.exhibitInstance = exhibitInstance;
        this.exhibitTileType = exhibitTileType;
    }
}
