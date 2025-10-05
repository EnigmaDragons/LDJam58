using System;
using System.Collections.Generic;
using System.Linq;
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

    public static void UpdatePlacedExhibit(ExhibitTileType exhibit, string roomId, Vector2Int[] nodes)
    {
        if (roomId == null)
        {
            Debug.LogError("Game State: UpdatePlacedExhibit: RoomId is null");
            return;
        }

        UpdateState(state =>
        {
            var adjacencies = new HashSet<Vector2Int>(); 
            foreach (var node in nodes)
            {
                if (!gameState.Rooms.ContainsKey(roomId))
                    gameState.Rooms[roomId] = new RoomState();
                if (!gameState.Rooms[roomId].exhibitIds.ContainsKey(node))
                    gameState.Rooms[roomId].exhibitIds[node] = exhibit.DisplayName;
                foreach (var vector in gameState.Rooms[roomId].exhibitIds.Keys.ToList())
                    state.Rooms[roomId].exhibitIds[vector] = exhibit.DisplayName;
                foreach (var vector in state.Rooms[roomId].exhibitIds.Keys.ToList())
                {
                    var directions = new Vector2Int[] { node + Vector2Int.up, node + Vector2Int.left, node + Vector2Int.left, node + Vector2Int.right };
                    if (directions.Any(x => x == vector))
                        adjacencies.Add(vector);
                }
            }
            state.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                name = exhibit.DisplayName,
                roomId = roomId,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                adjacencies = adjacencies.Where(x => nodes.All(node => node != x)).ToArray()
            };
        });
        CalculateExhibitEnjoyment(roomId);
    }

    public static void CalculateExhibitEnjoyment(string roomId)
    {
        UpdateState(state =>
        {
            var room = state.Rooms[roomId];
            var exhibitIds = room.exhibitIds.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x)).Distinct();
            foreach (var exhibitId in exhibitIds)
            {
                var exhibit = state.Exhibits[exhibitId];
                var adjacentExhibits = exhibit.adjacencies
                    .Select(x => room.exhibitIds[x])
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .Select(x => state.Exhibits[x]);
                exhibit.calculatedEnjoyment = exhibit.baseEnjoyment + adjacentExhibits.Sum(x => CalculateAdjacencyBonus(exhibit.tags, x.tags));
            }
        });
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
