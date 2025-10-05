using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExhibitState
{
    public int baseEnjoyment;
    public List<ExhibitTag> tags;
    public string roomId;
    public Vector2Int[] adjacencies;
}