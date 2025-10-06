using System.Linq;
using UnityEngine;

public class RoomTargettingCollider : MonoBehaviour
{
    [SerializeField] private GameObject[] nodes;
    [SerializeField] private GameObject centerOn;

    private string _roomId;
    public string RoomId => _roomId;
    public void Init(string roomId) => _roomId = roomId;

    public Vector2Int[] Nodes => nodes.Select(x => new Vector2Int((int)x.transform.position.x, (int)x.transform.position.z)).ToArray();
    public Vector3 Center => centerOn.transform.position;
}