using UnityEngine;

public class EntryRoomOpen : MonoBehaviour
{
    [SerializeField] private BoxCollider bounds;

    void Start() => Message.Publish(new RoomOpened {RoomId = "entry", Bounds = bounds.bounds});
}