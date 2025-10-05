using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomState
{
    public bool open;
    public Dictionary<Vector2Int, string> exhibitIds = new Dictionary<Vector2Int, string>();
}