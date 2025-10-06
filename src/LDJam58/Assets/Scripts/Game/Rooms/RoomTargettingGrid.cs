using UnityEngine;

public class RoomTargettingGrid : MonoBehaviour
{
    [SerializeField] private RoomTargettingCollider[] targets;

    public void Init(string roomId)
        => targets.ForEach(x => x.Init(roomId));
}