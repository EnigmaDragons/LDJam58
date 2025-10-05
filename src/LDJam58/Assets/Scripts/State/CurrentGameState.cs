using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public static class CurrentGameState
{
    [SerializeField] private static GameState gameState;

    public static GameState ReadOnly => gameState;

    public static void Init() => gameState = new GameState();
    public static void Init(GameState initialState) => gameState = initialState;
    public static void Subscribe(Action<GameStateChanged> onChange, object owner) => Message.Subscribe(onChange, owner);
    public static void Unsubscribe(object owner) => Message.Unsubscribe(owner);
    
    public static void UpdateState(Action<GameState> apply)
    {
        UpdateState(_ =>
        {
            apply(gameState);
            return gameState;
        });
    }
    
    public static void UpdateState(Func<GameState, GameState> apply)
    {
        gameState = apply(gameState);
        Message.Publish(new GameStateChanged(gameState));
    }

    public static void InitRoom(string roomId, bool open, Vector2Int[] nodes)
    {
        UpdateState(state => state.Rooms[roomId] = new RoomState { open = open, exhibitIds = nodes.ToDictionary(x => x, _ => "")});
    }

    public static void UpdatePlacedExhibit(ExhibitTileType exhibit, string roomId, Vector2Int[] nodes)
    {
        if (roomId == null)
        {
            Debug.LogError("Game State: UpdatePlacedExhibit: RoomId is null");
            return;
        }

        UpdateState(state =>
        {
            if (!state.Rooms.ContainsKey(roomId))
                state.Rooms[roomId] = new RoomState {open = true, exhibitIds = new Dictionary<Vector2Int, string>() };
            var adjacencies = new HashSet<Vector2Int>(); 
            foreach (var node in nodes)
            {
                gameState.Rooms[roomId].exhibitIds[node] = exhibit.DisplayName;
                adjacencies.AddRange(new Vector2Int[] { node + Vector2Int.up, node + Vector2Int.left, node + Vector2Int.down, node + Vector2Int.right });
            }
            var filteredAdjacentNodes = adjacencies.Where(x => nodes.All(node => node != x)).ToArray();
            state.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                name = exhibit.DisplayName,
                roomId = roomId,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                adjacencies = filteredAdjacentNodes
            };
        });
        CalculateExhibitEnjoyment(roomId);
    }

    public static void CalculateExhibitEnjoyment(string roomId)
    {
        UpdateState(state =>
        {
            var room = state.Rooms[roomId];
            var exhibitIds = room.exhibitIds.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
            foreach (var exhibitId in exhibitIds)
            {
                var exhibit = state.Exhibits[exhibitId];
                var adjacentExhibits = exhibit.adjacencies
                    .Where(x => room.exhibitIds.ContainsKey(x))
                    .Select(x => room.exhibitIds[x])
                    .Distinct()
                    .Select(x => state.Exhibits[x]);
                exhibit.calculatedEnjoyment = exhibit.baseEnjoyment + adjacentExhibits.Sum(x => CalculateAdjacencyBonus(exhibit.tags, x.tags));
            }
        });
        Message.Publish(new ScoresUpdated());
    }

    public static int CalculateAdjacencyBonus(List<ExhibitTag> exhibitTags, List<ExhibitTag> adjacentExhibitTags)
    {
        var synergy = 0;
        var disynergy = 0;
        foreach (var synergyTag in TagSynergies.All)
        {
            if ((exhibitTags.Contains(synergyTag.Tag1) && adjacentExhibitTags.Contains(synergyTag.Tag2)) 
                    || (exhibitTags.Contains(synergyTag.Tag2) && adjacentExhibitTags.Contains(synergyTag.Tag1)))
            {
                if (synergyTag.SynergyValue > 0)
                    synergy += synergyTag.SynergyValue;
                else
                    disynergy += synergyTag.SynergyValue;
            }
        }
        return synergy > 0 ? synergy : disynergy;
    }

    public static void CalculateRoundScore()
    {
        var groups = gameState.currentGroups.ToArray();
        foreach (var group in groups)
            CalculateGroupScore(group);
    }

    public static void CalculateGroupScore(Group group)
    {
        var exhibits = gameState.Exhibits.Values.ToArray();
        foreach (var exhibit in exhibits)
            ScoreGroupExhibit(group, exhibit);
    }

    public static void ScoreGroupExhibit(Group group, ExhibitState exhibit)
    {
        UpdateState(state =>
        {
            var groupInterest = 1 + group.Fascinations.Count(x => exhibit.tags.Contains(x)) - group.Disinterests.Count(x => exhibit.tags.Contains(x));
            if (groupInterest < 0)
                groupInterest = 0;
            var score = group.peopleCount * groupInterest * exhibit.calculatedEnjoyment;
            state.seasonScore += score;
            group.seasonScore += score;
            exhibit.seasonScore += score;
        });
    }
}
