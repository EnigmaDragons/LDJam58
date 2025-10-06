using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExhibitState
{
    public string name;
    public int baseEnjoyment;
    public List<ExhibitTag> tags = new List<ExhibitTag>();
    public string roomId;
    public bool isGhost;
    public Vector2Int[] adjacencies = new Vector2Int[0];
    public int calculatedEnjoyment;
    public int ghostEnjoyment;
    public int seasonScore;
}