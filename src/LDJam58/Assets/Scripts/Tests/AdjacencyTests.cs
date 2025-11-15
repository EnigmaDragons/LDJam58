using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AdjacencyTests
{
    // Real node configurations from prefabs
    // 2x2 exhibit nodes (from RoomBase.prefab analysis)
    private static readonly Vector2Int[] TwoByTwoNodes = new[]
    {
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)
    };
    
    // 3x3 exhibit nodes (from RoomBase.prefab analysis)
    private static readonly Vector2Int[] ThreeByThreeNodes = new[]
    {
        new Vector2Int(-2, 2),
        new Vector2Int(0, 2),
        new Vector2Int(2, 2),
        new Vector2Int(-2, 0),
        new Vector2Int(0, 0),
        new Vector2Int(2, 0),
        new Vector2Int(-2, -2),
        new Vector2Int(0, -2),
        new Vector2Int(2, -2)
    };
    
    // 2x3 exhibit nodes (from RoomBase.prefab analysis)
    private static readonly Vector2Int[] TwoByThreeNodes = new[]
    {
        new Vector2Int(-2, 1),
        new Vector2Int(0, 1),
        new Vector2Int(2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(0, -1),
        new Vector2Int(2, -1)
    };
    
    // Real node positions from logs (Spaceship Battle Model - 3x3)
    private static readonly Vector2Int[] SpaceshipBattleModelNodes = new[]
    {
        new Vector2Int(2, 26),
        new Vector2Int(4, 28),
        new Vector2Int(5, 29),
        new Vector2Int(5, 26),
        new Vector2Int(7, 28),
        new Vector2Int(4, 25),
        new Vector2Int(5, 24),
        new Vector2Int(7, 25),
        new Vector2Int(8, 26)
    };
    
    // Real node positions from logs (Cursed Coins - 2x2)
    private static readonly Vector2Int[] CursedCoinsNodes = new[]
    {
        new Vector2Int(7, 31),
        new Vector2Int(8, 32),
        new Vector2Int(8, 29),
        new Vector2Int(9, 31)
    };
    
    // Real node positions from logs (Black Hole - 2x2)
    private static readonly Vector2Int[] BlackHoleNodes = new[]
    {
        new Vector2Int(11, 26),
        new Vector2Int(12, 28),
        new Vector2Int(9, 28),
        new Vector2Int(11, 29)
    };

    [Test]
    public void AreNodesAdjacent_Distance1_SameRow_ReturnsTrue()
    {
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(1, 0)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(6, 10)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(10, 5), new Vector2Int(11, 5)));
    }

    [Test]
    public void AreNodesAdjacent_Distance1_SameColumn_ReturnsTrue()
    {
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(0, 1)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(5, 11)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(10, 5), new Vector2Int(10, 6)));
    }

    [Test]
    public void AreNodesAdjacent_Distance2_SameRow_ReturnsTrue()
    {
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(2, 0)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(7, 10)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(10, 5), new Vector2Int(12, 5)));
    }

    [Test]
    public void AreNodesAdjacent_Distance2_SameColumn_ReturnsTrue()
    {
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(0, 2)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(5, 12)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(10, 5), new Vector2Int(10, 7)));
    }

    [Test]
    public void AreNodesAdjacent_Diagonal_Distance2_ReturnsFalse()
    {
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(1, 1)));
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(6, 11)));
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(10, 5), new Vector2Int(11, 6)));
    }

    [Test]
    public void AreNodesAdjacent_Distance3_ReturnsFalse()
    {
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(3, 0)));
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(0, 3)));
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(8, 10)));
    }

    [Test]
    public void AreNodesAdjacent_SameNode_ReturnsFalse()
    {
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 0), new Vector2Int(0, 0)));
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(5, 10), new Vector2Int(5, 10)));
    }

    [Test]
    public void AreNodesAdjacent_TwoByTwoExhibit_InternalNodes_AdjacentPairs()
    {
        // Test that 2x2 exhibit nodes that should be adjacent are detected
        // Top row: (-1,1) and (1,1) - distance 2, same row
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[0], TwoByTwoNodes[1]));
        
        // Left column: (-1,1) and (-1,-1) - distance 2, same column
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[0], TwoByTwoNodes[2]));
        
        // Right column: (1,1) and (1,-1) - distance 2, same column
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[1], TwoByTwoNodes[3]));
        
        // Bottom row: (-1,-1) and (1,-1) - distance 2, same row
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[2], TwoByTwoNodes[3]));
    }

    [Test]
    public void AreNodesAdjacent_TwoByTwoExhibit_DiagonalPairs_NotAdjacent()
    {
        // Diagonal pairs should NOT be adjacent
        // Top-left to bottom-right: (-1,1) to (1,-1)
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[0], TwoByTwoNodes[3]));
        
        // Top-right to bottom-left: (1,1) to (-1,-1)
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(TwoByTwoNodes[1], TwoByTwoNodes[2]));
    }

    [Test]
    public void AreNodesAdjacent_ThreeByThreeExhibit_InternalNodes_AdjacentPairs()
    {
        // Test center node (0,0) with its neighbors
        var center = new Vector2Int(0, 0);
        
        // Center to north: (0,0) to (0,2) - distance 2, same column
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(0, 2)));
        
        // Center to south: (0,0) to (0,-2) - distance 2, same column
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(0, -2)));
        
        // Center to east: (0,0) to (2,0) - distance 2, same row
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(2, 0)));
        
        // Center to west: (0,0) to (-2,0) - distance 2, same row
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(-2, 0)));
    }

    [Test]
    public void AreNodesAdjacent_ThreeByThreeExhibit_DiagonalPairs_NotAdjacent()
    {
        var center = new Vector2Int(0, 0);
        
        // Diagonal neighbors should NOT be adjacent
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(-2, 2))); // NW
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(2, 2)));   // NE
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(-2, -2))); // SW
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(center, new Vector2Int(2, -2)));  // SE
    }

    [Test]
    public void AreNodesAdjacent_TwoByThreeExhibit_AdjacentPairs()
    {
        // Test horizontal neighbors in same row
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(-2, 1), new Vector2Int(0, 1)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 1), new Vector2Int(2, 1)));
        
        // Test vertical neighbors in same column
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(-2, 1), new Vector2Int(-2, -1)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(0, 1), new Vector2Int(0, -1)));
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(2, 1), new Vector2Int(2, -1)));
    }

    [Test]
    public void AreNodesAdjacent_RealWorld_SpaceshipBattleModel_To_CursedCoins()
    {
        // From logs: (7, 28) from Spaceship Battle Model should be adjacent to (8, 29) from Cursed Coins
        // Distance: |7-8| + |28-29| = 1 + 1 = 2, but diagonal - should NOT be adjacent
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(7, 28), new Vector2Int(8, 29)));
        
        // But (7, 28) should be adjacent to (8, 28) if it existed (distance 1, same row)
        // And (7, 28) should be adjacent to (7, 29) if it existed (distance 1, same column)
        // And (7, 28) should be adjacent to (9, 28) (distance 2, same row) - this matches Black Hole!
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(7, 28), new Vector2Int(9, 28)));
    }

    [Test]
    public void AreNodesAdjacent_RealWorld_SpaceshipBattleModel_To_BlackHole()
    {
        // From logs: (7, 28) from Spaceship Battle Model should be adjacent to (9, 28) from Black Hole
        // Distance: |7-9| + |28-28| = 2 + 0 = 2, same row - SHOULD be adjacent
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(new Vector2Int(7, 28), new Vector2Int(9, 28)));
        
        // (8, 26) to (11, 26): distance 3, same row - should NOT be adjacent
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(8, 26), new Vector2Int(11, 26)));
    }

    [Test]
    public void AreNodesAdjacent_RealWorld_CursedCoins_To_BlackHole()
    {
        // From logs: (8, 29) from Cursed Coins should be adjacent to (9, 28) from Black Hole
        // Distance: |8-9| + |29-28| = 1 + 1 = 2, but diagonal - should NOT be adjacent
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(new Vector2Int(8, 29), new Vector2Int(9, 28)));
        
        // But (8, 29) should be adjacent to (9, 29) if it existed (distance 1, same row)
        // And (8, 29) should be adjacent to (8, 28) if it existed (distance 1, same column)
    }

    [Test]
    public void AreNodesAdjacent_AllTwoByTwoPairs()
    {
        // Test all pairs in 2x2 exhibit
        for (var i = 0; i < TwoByTwoNodes.Length; i++)
        {
            for (var j = i + 1; j < TwoByTwoNodes.Length; j++)
            {
                var node1 = TwoByTwoNodes[i];
                var node2 = TwoByTwoNodes[j];
                var dx = Mathf.Abs(node1.x - node2.x);
                var dy = Mathf.Abs(node1.y - node2.y);
                var isDiagonal = dx == dy && dx > 0;
                var isCardinal = (dx == 0 && dy > 0) || (dy == 0 && dx > 0);
                var distance = dx + dy;
                var expectedAdjacent = isCardinal && (distance == 1 || distance == 2);
                
                Assert.AreEqual(expectedAdjacent, CurrentGameState.AreNodesAdjacent(node1, node2),
                    $"Nodes {node1} and {node2} (dx={dx}, dy={dy}, distance={distance}, diagonal={isDiagonal}, cardinal={isCardinal})");
            }
        }
    }

    [Test]
    public void AreNodesAdjacent_AllThreeByThreePairs()
    {
        // Test all pairs in 3x3 exhibit
        for (var i = 0; i < ThreeByThreeNodes.Length; i++)
        {
            for (var j = i + 1; j < ThreeByThreeNodes.Length; j++)
            {
                var node1 = ThreeByThreeNodes[i];
                var node2 = ThreeByThreeNodes[j];
                var dx = Mathf.Abs(node1.x - node2.x);
                var dy = Mathf.Abs(node1.y - node2.y);
                var isDiagonal = dx == dy && dx > 0;
                var isCardinal = (dx == 0 && dy > 0) || (dy == 0 && dx > 0);
                var distance = dx + dy;
                var expectedAdjacent = isCardinal && (distance == 1 || distance == 2);
                
                Assert.AreEqual(expectedAdjacent, CurrentGameState.AreNodesAdjacent(node1, node2),
                    $"Nodes {node1} and {node2} (dx={dx}, dy={dy}, distance={distance}, diagonal={isDiagonal}, cardinal={isCardinal})");
            }
        }
    }

    [Test]
    public void AreNodesAdjacent_CrossExhibit_RealWorldScenarios()
    {
        // Test real-world scenarios where exhibits should be adjacent
        // Spaceship Battle Model (3x3) adjacent to Black Hole (2x2)
        var spaceshipNode = new Vector2Int(7, 28);
        var blackHoleNode = new Vector2Int(9, 28);
        Assert.IsTrue(CurrentGameState.AreNodesAdjacent(spaceshipNode, blackHoleNode),
            $"Spaceship Battle Model node {spaceshipNode} should be adjacent to Black Hole node {blackHoleNode}");
        
        // Test that nodes that are too far apart are not adjacent
        var spaceshipNode2 = new Vector2Int(8, 26);
        var blackHoleNode2 = new Vector2Int(11, 26);
        Assert.IsFalse(CurrentGameState.AreNodesAdjacent(spaceshipNode2, blackHoleNode2),
            $"Spaceship Battle Model node {spaceshipNode2} should NOT be adjacent to Black Hole node {blackHoleNode2} (distance 3)");
    }
}

