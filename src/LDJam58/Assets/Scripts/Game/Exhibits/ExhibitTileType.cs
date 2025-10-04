using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExhibitTileType
{
    public string DisplayName;        
    public List<ExhibitTag> Tags;
    public Vector2Int Size;
    public ExhibitRarity Rarity;
    public int Enjoyment;
    public int Popularity;


    public GameObject ExhibitPrefab => LoadingUtils.LoadPrefabOrDefault(DisplayName, Size);
    
    public Sprite ExhibitSprite => LoadingUtils.LoadSpriteOrDefault(DisplayName); 
}

