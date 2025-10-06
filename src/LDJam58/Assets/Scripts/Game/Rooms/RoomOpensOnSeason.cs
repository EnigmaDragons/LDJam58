using UnityEngine;

public class RoomOpensOnSeason : OnMessage<SeasonInitialized>
{
    [SerializeField] private RoomId roomId;
    [SerializeField] private GameObject ceiling;
    [SerializeField] private GameObject placementGrid;
    [SerializeField] private int seasonToOpen = 1;

    private bool presentlyOpen = true;
    
    protected override void Execute(SeasonInitialized msg)
    {
        CurrentGameState.UpdateState(state =>
        {
            if (!state.Rooms.ContainsKey(roomId.Id))
                state.Rooms[roomId.Id] = new RoomState();
            var open = state.currentSeasonIndex >= seasonToOpen - 1;
            if (open == presentlyOpen)
                return;
            presentlyOpen = open;
            state.Rooms[roomId.Id].open = open;
            ceiling.SetActive(!presentlyOpen);
            placementGrid.SetActive(presentlyOpen);
        });
    }
}