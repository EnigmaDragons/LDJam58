using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomState
{
    public bool open { get; set; }
    public Dictionary<Vector2Int, string> exhibitIds = new Dictionary<Vector2Int, string>();
    public int seasonScore;
}