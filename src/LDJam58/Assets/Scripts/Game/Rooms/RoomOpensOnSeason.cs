using UnityEngine;

public class RoomOpensOnSeason : OnMessage<SeasonInitialized>
{
    [SerializeField] private RoomId roomId;
    [SerializeField] private GameObject ceiling;
    [SerializeField] private GameObject placementGrid;
    [SerializeField] private BoxCollider boundsCollider;
    [SerializeField] private int seasonToOpen = 1;

    private bool init = false;
    private bool presentlyOpen = true;
    
    protected override void Execute(SeasonInitialized msg)
    {
        var changed = false;
        CurrentGameState.UpdateState(state =>
        {
            if (!state.Rooms.ContainsKey(roomId.Id))
                state.Rooms[roomId.Id] = new RoomState();
            var open = state.currentSeasonIndex >= seasonToOpen - 1;
            if (init && open == presentlyOpen)
                return;
            changed = true;
            presentlyOpen = open;
            state.Rooms[roomId.Id].open = open;
            ceiling.SetActive(!presentlyOpen);
            placementGrid.SetActive(presentlyOpen);
        });
        if (changed && presentlyOpen)
            Message.Publish(new RoomOpened { RoomId = roomId.Id, Bounds = boundsCollider.bounds });
        init = true;
    }
}