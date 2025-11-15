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
    
    /// <summary>
    /// Checks if two grid nodes are adjacent (touching edges).
    /// Two nodes are adjacent if they are cardinal neighbors (north/south/east/west).
    /// Since the grid uses 2-unit cells but nodes can be at 1-unit intervals,
    /// we check for Manhattan distance of 1 or 2, but ONLY cardinal (not diagonal).
    /// </summary>
    /// <param name="node1">First node position</param>
    /// <param name="node2">Second node position</param>
    /// <returns>True if the nodes are adjacent (edge-touching), false otherwise</returns>
    public static bool AreNodesAdjacent(Vector2Int node1, Vector2Int node2)
    {
        var dx = Mathf.Abs(node1.x - node2.x);
        var dy = Mathf.Abs(node1.y - node2.y);
        
        // Cardinal neighbors: must be in same row (dy == 0) OR same column (dx == 0)
        // And distance must be 1 or 2 units
        var isCardinal = (dx == 0 && dy > 0) || (dy == 0 && dx > 0);
        var distance = dx + dy;
        
        return isCardinal && (distance == 1 || distance == 2);
    }
    
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
            state.focusedRoom = roomId;
            //remove ghosts
            foreach (var room in state.Rooms.Values)
            {
                var nodes = room.exhibitIds.ToArray();
                foreach (var node in nodes.Where(x => !string.IsNullOrEmpty(x.Value)))
                {
                    var exhibit = state.Exhibits[node.Value];
                    if (exhibit.isGhost)
                    {
                        roomsToRecalculate.Add(exhibit.roomId);                        
                        room.exhibitIds[node.Key] = "";
                    }
                }
            }
            
            var exhibitRoom = state.Rooms[roomId];
            var adjacencies = new HashSet<Vector2Int>();
            Debug.Log($"UpdatePlacedExhibit: {exhibit.DisplayName} occupies nodes: {string.Join(", ", nodes.Select(n => n.ToString()))}");
            foreach (var node in nodes)
            {
                exhibitRoom.exhibitIds[node] = exhibit.DisplayName;
            }
            
            // Check all room nodes to find adjacent exhibits
            // Two exhibits are adjacent if any of their nodes are edge-touching (cardinal neighbors: distance 1 or 2, but not diagonal)
            foreach (var node in nodes)
            {
                foreach (var roomNode in exhibitRoom.exhibitIds.Keys)
                {
                    // Skip if it's one of our own nodes
                    if (nodes.Any(x => x == roomNode))
                        continue;
                    
                    // Check if this room node is adjacent (edge-touching)
                    if (!AreNodesAdjacent(node, roomNode))
                        continue;
                    
                    var adjacentExhibitName = exhibitRoom.exhibitIds[roomNode];
                    if (string.IsNullOrEmpty(adjacentExhibitName))
                        continue;
                    
                    Debug.Log($"  Found adjacent exhibit '{adjacentExhibitName}' at {roomNode} (adjacent to {node})");
                    adjacencies.Add(roomNode);
                }
            }
            Debug.Log($"  Total adjacencies found: {adjacencies.Count}");
            state.Exhibits[exhibit.DisplayName] = new ExhibitState
            {
                name = exhibit.DisplayName,
                roomId = roomId,
                tags = exhibit.Tags,
                baseEnjoyment = exhibit.Enjoyment,
                adjacencies = adjacencies.ToArray(),
                isGhost = isGhost
            };
        });
        
        // Check for room transformation (only if not ghost placement)
        if (!isGhost)
            CheckAndApplyRoomTransformation(roomId);
        
        CalculateExhibitEnjoyment(roomsToRecalculate);
    }
    
    private static void CheckAndApplyRoomTransformation(string roomId)
    {
        UpdateState(state =>
        {
            var room = state.Rooms[roomId];
            if (room.roomType != RoomPool.Basic) // Already transformed (permanent)
                return;
            
            // Count all tags from all exhibits in room (single iteration)
            var tagCounts = new Dictionary<ExhibitTag, int>();
            foreach (var exhibitId in room.exhibitIds.Values.Where(x => !string.IsNullOrEmpty(x)).Distinct())
            {
                var exhibit = state.Exhibits[exhibitId];
                if (exhibit.isGhost) continue;
                
                foreach (var tag in exhibit.tags)
                {
                    if (!tagCounts.ContainsKey(tag))
                        tagCounts[tag] = 0;
                    tagCounts[tag]++;
                }
            }
            
            // Check RoomPool.All in order, take first match that isn't already assigned
            foreach (var roomType in RoomPool.All)
            {
                // Skip if this room type is already assigned to another room (each type can only exist once)
                var isAlreadyAssigned = state.Rooms.Values.Any(r => r.roomType == roomType && r != room);
                if (isAlreadyAssigned)
                    continue;
                
                // Check if all requirements met by counting required tags
                var meetsRequirements = true;
                var requirementCounts = new Dictionary<ExhibitTag, int>();
                
                foreach (var tag in roomType.Requirement)
                {
                    if (!requirementCounts.ContainsKey(tag))
                        requirementCounts[tag] = 0;
                    requirementCounts[tag]++;
                }
                
                foreach (var requirement in requirementCounts)
                {
                    if (!tagCounts.ContainsKey(requirement.Key) || tagCounts[requirement.Key] < requirement.Value)
                    {
                        meetsRequirements = false;
                        break;
                    }
                }
                
                if (meetsRequirements)
                {
                    room.roomType = roomType;
                    Message.Publish(new RoomTransformed(roomId, roomType));
                    return;
                }
            }
        });
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
                    // Find all nodes that belong to this exhibit
                    var exhibitNodes = room.exhibitIds.Where(x => x.Value == exhibitId).Select(x => x.Key).ToArray();
                    Debug.Log($"CalculateEnjoyment for {exhibitId}: occupies nodes = {string.Join(", ", exhibitNodes.Select(a => a.ToString()))}");
                    
                    // Dynamically find adjacent exhibits by checking all room nodes
                    // Two exhibits are adjacent if any of their nodes are edge-touching (cardinal neighbors: distance 1 or 2, but not diagonal)
                    var adjacentExhibitNames = new HashSet<string>();
                    foreach (var node in exhibitNodes)
                    {
                        foreach (var roomNode in room.exhibitIds.Keys)
                        {
                            // Skip if it's one of our own nodes
                            if (exhibitNodes.Any(x => x == roomNode))
                                continue;
                            
                            // Check if this room node is adjacent (edge-touching)
                            var isAdjacent = AreNodesAdjacent(node, roomNode);
                            if (!isAdjacent)
                            {
                                // Debug: log close but not adjacent nodes
                                var distance = Mathf.Abs(node.x - roomNode.x) + Mathf.Abs(node.y - roomNode.y);
                                if (distance <= 3)
                                {
                                    var nearbyExhibitName = room.exhibitIds[roomNode];
                                    if (!string.IsNullOrEmpty(nearbyExhibitName))
                                    {
                                        var dx = Mathf.Abs(node.x - roomNode.x);
                                        var dy = Mathf.Abs(node.y - roomNode.y);
                                        var isDiagonal = dx == dy && dx > 0;
                                        var reason = isDiagonal ? "diagonal (not cardinal)" : $"distance {distance} (needs 1 or 2, cardinal only)";
                                        Debug.Log($"  Node {node} is distance {distance} from {roomNode} (exhibit: {nearbyExhibitName}) - NOT adjacent ({reason})");
                                    }
                                }
                                continue;
                            }
                            
                            var adjacentExhibitName = room.exhibitIds[roomNode];
                            if (string.IsNullOrEmpty(adjacentExhibitName))
                                continue;
                            
                            Debug.Log($"  Found adjacent node pair: {node} <-> {roomNode} (exhibit: {adjacentExhibitName})");
                            adjacentExhibitNames.Add(adjacentExhibitName);
                        }
                    }
                    
                    var adjacentExhibits = adjacentExhibitNames.Select(x => state.Exhibits[x]).ToArray();
                    Debug.Log($"  Adjacent exhibits: {string.Join(", ", adjacentExhibits.Select(e => e.name))}");
                    var bonusTotal = adjacentExhibits
                        .Where(x => !x.isGhost)
                        .Sum(adjacentExhibit => CalculateAdjacencyBonus(exhibit.tags, adjacentExhibit.tags).Sum(x => x.Item2));
                    var ghostBonus = adjacentExhibits
                        .Where(x => x.isGhost)
                        .Sum(adjacentExhibit => CalculateAdjacencyBonus(exhibit.tags, adjacentExhibit.tags).Sum(x => x.Item2));
                    Debug.Log($"  Base enjoyment: {exhibit.baseEnjoyment}, Bonus: {bonusTotal}, Ghost Bonus: {ghostBonus}, Total: {exhibit.baseEnjoyment + bonusTotal}");
                    exhibit.calculatedEnjoyment = Math.Max(0, exhibit.baseEnjoyment + bonusTotal);
                    exhibit.ghostEnjoyment = Math.Max(0, exhibit.calculatedEnjoyment + ghostBonus);
                    Debug.Log($"  Final values - calculatedEnjoyment: {exhibit.calculatedEnjoyment}, ghostEnjoyment: {exhibit.ghostEnjoyment}");
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
                || (x.Tag2 == adjacentTag && exhibitTags.Contains(x.Tag1))).ToArray();
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
            
            // Apply room transformation multiplier ONLY if exhibit has one of the room's feature tags
            var room = state.Rooms[exhibit.roomId];
            var multiplier = 1;
            if (room.roomType != RoomPool.Basic)
            {
                var roomFeatureTags = room.roomType.Requirement.Distinct();
                var hasFeatureTag = exhibit.tags.Any(tag => roomFeatureTags.Contains(tag));
                if (hasFeatureTag)
                    multiplier = room.roomType.Multiplier;
            }
            
            var score = group.peopleCount * groupInterest * exhibit.calculatedEnjoyment * multiplier;
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
                        room.exhibitIds[node.Key] = "";
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
