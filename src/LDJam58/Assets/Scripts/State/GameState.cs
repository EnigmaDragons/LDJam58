using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameState
{
    // Should consist of only serializable primitives.
    // Any logic or non-trivial data should be enriched in CurrentGameState.
    // Except for Save/Load Systems, everything should use CurrentGameState,
    // instead of this pure data structure.
    
    // All enums used in this class should have specified integer values.
    // This is necessary to preserve backwards save compatibility.
    public ProgressionConfig progressionConfig;
    public int currentSeasonIndex = 0;
    public int currentNumExhibitsToPickThisPeriod = 0;
    public int currentTargetAppeal = 0;
    public List<Group> currentGroups = new List<Group>();
    public Dictionary<string, RoomState> Rooms = new Dictionary<string, RoomState>();
    public Dictionary<string, ExhibitState> Exhibits = new Dictionary<string, ExhibitState>();
    public string focusedExhibit; //while picking this will be the ghost
    public string focusedRoom;
    public bool showDetails;
    public int seasonScore;
    public string GuaranteedPick;
    public int MythicChance;
    public int ExoticChance;
    public int RareChance;
    
    public bool isPicking = false;
    public bool isPlacing = false;
    public bool isShowingMuseum = false;
}
