using UnityEngine;

public class EntryRoomOpen : MonoBehaviour
{
    [SerializeField] private BoxCollider bounds;

    void Start()
    {
        CurrentGameState.UpdateState(state =>
        {
            if (!state.Rooms.ContainsKey("entry"))
                state.Rooms["entry"] = new RoomState();
            state.Rooms["entry"].open = true;
        });
        Message.Publish(new RoomOpened {RoomId = "entry", Bounds = bounds.bounds});
    }
}