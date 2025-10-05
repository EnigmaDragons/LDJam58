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

        UpdateState(_ =>
        {
            var adjacencies = new HashSet<Vector2Int>(); 
            foreach (var node in nodes)
            {
                if (!gameState.Rooms.ContainsKey(roomId))
                    gameState.Rooms[roomId] = new RoomState();
                if (!gameState.Rooms[roomId].exhibitIds.ContainsKey(node))
                    gameState.Rooms[roomId].exhibitIds[node] = exhibit.DisplayName;
                gameState.Rooms[roomId].exhibitIds[node] = exhibit.DisplayName;
                foreach (var vector in gameState.Rooms[roomId].exhibitIds.Keys)
                {
                    var directions = new Vector2Int[] { node + Vector2Int.up, node + Vector2Int.left, node + Vector2Int.left, node + Vector2Int.right };
                    if (directions.Any(x => x == vector))
                        adjacencies.Add(vector);
                }
            }
            gameState.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                roomId = roomId,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                adjacencies = adjacencies.Where(x => nodes.All(node => node != x)).ToArray()
            };
        });
    }
}
