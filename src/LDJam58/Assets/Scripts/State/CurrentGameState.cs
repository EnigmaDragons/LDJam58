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
        UpdateState(state =>
        {
            var adjacencies = new HashSet<Vector2Int>(); 
            foreach (var node in nodes)
            {
                state.Rooms[roomId].exhibitIds[node] = exhibit.DisplayName;
                foreach (var vector in state.Rooms[roomId].exhibitIds.Keys)
                {
                    var directions = new Vector2Int[] { node + Vector2Int.up, node + Vector2Int.left, node + Vector2Int.left, node + Vector2Int.right };
                    if (directions.Any(x => x == vector))
                        adjacencies.Add(vector);
                }
            }
            state.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                roomId = roomId,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                adjacencies = adjacencies.Where(x => nodes.All(node => node != x)).ToArray()
            };
        });
        CalculateEnjoyment(roomId);
    }
    
    public static void CalculateEnjoyment(string roomId)
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
}
