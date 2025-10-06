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

    public static void UpdatePlacedExhibit(ExhibitTileType exhibit, string roomId, Vector2Int[] nodes, bool isGhost)
    {
        if (roomId == null)
        {
            Debug.LogError("Game State: UpdatePlacedExhibit: RoomId is null");
            return;
        }

        var roomsToRecalculate = new HashSet<string>() { roomId };

        UpdateState(state =>
        {
            if (!state.Rooms.ContainsKey(roomId))
                state.Rooms[roomId] = new RoomState {open = true, exhibitIds = new Dictionary<Vector2Int, string>() };
            state.focusedRoom = roomId;
            //remove ghosts
            foreach (var room in state.Rooms.Values)
            {
                var nodes = room.exhibitIds.ToArray();
                foreach (var node in nodes)
                {
                    var exhibit = state.Exhibits[node.Value];
                    if (exhibit.isGhost)
                    {
                        roomsToRecalculate.Add(exhibit.roomId);                        
                        room.exhibitIds.Remove(node.Key);
                    }
                }
            }
            
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
                adjacencies = filteredAdjacentNodes,
                isGhost = isGhost
            };
        });
        CalculateExhibitEnjoyment(roomsToRecalculate);
    }
    
    public static void CalculateExhibitEnjoyment(IEnumerable<string> roomIds)
    {
        UpdateState(state =>
        {
            foreach (var room in state.Rooms.Where(x => roomIds.Contains(x.Key)).Select(x => x.Value))
            {
                var exhibitIds = room.exhibitIds.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
                foreach (var exhibitId in exhibitIds)
                {
                    var exhibit = state.Exhibits[exhibitId];
                    var adjacentExhibits = exhibit.adjacencies
                        .Where(x => room.exhibitIds.ContainsKey(x))
                        .Select(x => room.exhibitIds[x])
                        .Distinct()
                        .Select(x => state.Exhibits[x])
                        .ToArray();
                    exhibit.calculatedEnjoyment = Math.Max(0, exhibit.baseEnjoyment + adjacentExhibits
                        .Where(x => !x.isGhost)
                        .Sum(adjacentExhibit => CalculateAdjacencyBonus(exhibit.tags, adjacentExhibit.tags).Sum(x => x.Item2)));
                    exhibit.ghostEnjoyment = Math.Max(0, exhibit.calculatedEnjoyment + adjacentExhibits
                        .Where(x => x.isGhost)
                        .Sum(adjacentExhibit => CalculateAdjacencyBonus(exhibit.tags, adjacentExhibit.tags).Sum(x => x.Item2)));
                }
            }
        });
        Message.Publish(new ScoresUpdated());
    }

    public static List<(ExhibitTag, int)> CalculateAdjacencyBonus(List<ExhibitTag> exhibitTags, List<ExhibitTag> adjacentExhibitTags)
    {
        var positiveResults = new List<(ExhibitTag, int)>();
        var negativeResults = new List<(ExhibitTag, int)>();
        foreach (var adjacentTag in adjacentExhibitTags)
        {
            var synergies = TagSynergies.All.Where(x 
                => (x.Tag1 == adjacentTag && exhibitTags.Contains(x.Tag2)) 
                || (x.Tag2 == adjacentTag && exhibitTags.Contains(x.Tag2))).ToArray();
            if (!synergies.Any())
                continue;
            var bestSynergy = synergies.Max(x => x.SynergyValue);
            if (bestSynergy > 0)
                positiveResults.Add(new (adjacentTag, bestSynergy));
            else
            {
                var worstDisynergy = synergies.Min(x => x.SynergyValue);
                negativeResults.Add(new (adjacentTag, worstDisynergy));
            }
        }
        return positiveResults.Any() ? positiveResults : negativeResults;
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
            //group.ExhibitReactions[exhibit.name];
            group.ScoredExhibits[exhibit.name] = score;
            exhibit.seasonScore += score;
        });
    }
    
    public static int GetExhibitEnjoymentScore(ExhibitPrefab exhibit)
    {
        return ReadOnly.Exhibits[exhibit.ExhibitTileType.DisplayName].calculatedEnjoyment;
    }

    public static void InvalidGhostPlacement(ExhibitTileType exhibit)
    {
        string roomId = "";
        UpdateState(state =>
        {
            if (state.Exhibits.ContainsKey(exhibit.DisplayName) && !string.IsNullOrEmpty(state.Exhibits[exhibit.DisplayName].roomId))
            {
                roomId = state.Exhibits[exhibit.DisplayName].roomId;
                var room = state.Rooms[roomId];
                var nodes = room.exhibitIds.ToArray();
                    foreach (var node in nodes.Where(x => x.Value == exhibit.DisplayName))
                        room.exhibitIds.Remove(node.Key);
            }
            state.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                name = exhibit.DisplayName,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                isGhost = true
            };
            state.focusedExhibit = exhibit.DisplayName;
        });
        if (string.IsNullOrEmpty(roomId))
            CalculateExhibitEnjoyment(new [] { roomId });
    }

    public static int GetGhostExhibitEnjoymentScore(ExhibitTileType exhibit, string roomId, Vector2Int[] nodes)
    {
        UpdatePlacedExhibit(exhibit, roomId, nodes, true);
        return ReadOnly.Exhibits[exhibit.DisplayName].calculatedEnjoyment;
    }

    public static int GetExhibitBaseScore(ExhibitTileType exhibit)
    {
        return exhibit.Enjoyment;
    }
}
