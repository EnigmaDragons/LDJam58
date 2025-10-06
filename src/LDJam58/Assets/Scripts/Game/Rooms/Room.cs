using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : OnMessage<StartPlacement, RotationChanged, StopPlacement>
{
    [SerializeField] private RoomId id;
    [SerializeField] private GameObject nodeParentParent;
    [SerializeField] private RoomTargettingGrid twoTwoTargets;
    [SerializeField] private RoomTargettingGrid twoThreeTargets;
    [SerializeField] private RoomTargettingGrid threeTwoTargets;
    [SerializeField] private RoomTargettingGrid threeThreeTargets;

    private Vector2Int _target;
    
    private void Start()
    {
        twoTwoTargets.Init(id.Id);
        twoThreeTargets.Init(id.Id);
        threeTwoTargets.Init(id.Id);
        threeThreeTargets.Init(id.Id);
        var nodes = new List<Vector2Int>();
        foreach (Transform child in nodeParentParent.transform)
            foreach (Transform node in child)
                nodes.Add(new Vector2Int((int)node.position.x, (int)node.position.z));
        CurrentGameState.UpdateState(state => state.Rooms[id.Id] = new RoomState { exhibitIds = nodes.ToDictionary(x => x, _ => "") });
    }

    protected override void Execute(StartPlacement msg)
    {
        _target = msg.exhibit.Size;
        if (msg.exhibit.Size == new Vector2Int(2, 2))
            twoTwoTargets.gameObject.SetActive(true);
        else if (msg.exhibit.Size == new Vector2Int(2, 3))
            twoThreeTargets.gameObject.SetActive(true);
        else if (msg.exhibit.Size == new Vector2Int(3, 2))
            threeTwoTargets.gameObject.SetActive(true);
        else if (msg.exhibit.Size == new Vector2Int(3, 3))
            threeThreeTargets.gameObject.SetActive(true);
    }

    protected override void Execute(RotationChanged msg)
    {
        if (_target == new Vector2Int(2, 3))
        {
            _target = new Vector2Int(3, 2);
            twoThreeTargets.gameObject.SetActive(false);
            threeTwoTargets.gameObject.SetActive(true);
        }
        else if (_target == new Vector2Int(3, 2))
        {
            _target = new Vector2Int(2, 3);
            twoThreeTargets.gameObject.SetActive(true);
            threeTwoTargets.gameObject.SetActive(false);
        }
    }

    protected override void Execute(StopPlacement msg)
    {
        _target = Vector2Int.zero;
        twoTwoTargets.gameObject.SetActive(false);
        twoThreeTargets.gameObject.SetActive(false);
        threeTwoTargets.gameObject.SetActive(false);
        threeThreeTargets.gameObject.SetActive(false);
    }
}