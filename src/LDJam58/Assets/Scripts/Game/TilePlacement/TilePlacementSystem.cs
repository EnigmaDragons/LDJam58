using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.TilePlacement
{
    public class TilePlacementSystem : OnMessage<StartPlacement, StopPlacement>
    {
        private const bool debug = false;

        [Header("Scene objects")]
        [SerializeField]
        private Camera raycastCamera;
        [SerializeField]
        private Grid grid;

        [Header("Settings")] 
        [SerializeField] private float rotationAngle;
        [SerializeField]
        private LayerMask placementLayerMask;
        
        
        [Header("Materials")]
        [SerializeField]
        private GhostTile ghostTile;
        
        private ExhibitTileType  exhibitTileType;
        
        private PlacementState currentState;
        
        private Quaternion targetRotation;
        private Material currentTileMaterial;

        
        private enum PlacementState
        {
            Disabled,
            NoTarget,
            GhostPlacement
        }

        [Button]
        public void StartPlacing()
        {
            CurrentGameState.UpdateState(gs => {
                gs.isPlacing = true;
                return gs;
            });
            if(raycastCamera == null) raycastCamera = Camera.main;
            targetRotation = Quaternion.identity;
            ghostTile.transform.rotation = targetRotation; // Reset ghost tile rotation
            currentState = PlacementState.NoTarget;
            if(exhibitTileType != null && exhibitTileType.ExhibitPrefab == null) 
                Log.Error("Could Not Load Exhibit Prefab for " + exhibitTileType.DisplayName);
            if (exhibitTileType != null)
                ghostTile.UpdatePlaceable(exhibitTileType.ExhibitPrefab);
            Message.Publish(new RotationChanged(targetRotation.eulerAngles.y));
        }

        [Button]
        public void StopPlacing()
        {
            currentState = PlacementState.Disabled;
        }
        
        private void Update()
        {
            if(currentState == PlacementState.Disabled) return;
            
            HandlePlacement();
            HandleRotation();
        }

        private void HandleRotation()
        {
            if(currentState != PlacementState.GhostPlacement) return;
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                targetRotation *= Quaternion.Euler(0f, -90f, 0f);
                ghostTile.transform.rotation = targetRotation;
                Message.Publish(new RotationChanged(targetRotation.eulerAngles.y));
            }
            
            if (Input.GetMouseButtonDown(1)) // Right mouse button
            {
                targetRotation *= Quaternion.Euler(0f, 90f, 0f); // Clockwise rotation
                ghostTile.transform.rotation = targetRotation;
                Message.Publish(new RotationChanged(targetRotation.eulerAngles.y));
            }
        }
        
        private void HandlePlacement()
        {
            var ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            var isHit = Physics.Raycast(ray, out hit,  Mathf.Infinity, placementLayerMask);
            if (!isHit)
            {
                if(currentState == PlacementState.GhostPlacement) DisableGhostObject();
                currentState = PlacementState.NoTarget;
                return;
            }
            
            if(currentState == PlacementState.NoTarget) EnableGhostObject();
            currentState = PlacementState.GhostPlacement;
            
            var cellSize = grid.cellSize;
            var targetCell = grid.WorldToCell(hit.point+cellSize/2);
            var targetPosition = grid.CellToWorld(targetCell);
            
            //make a ray from target position and upward
            
            ghostTile.transform.position = targetPosition;

            if(ghostTile.IsOverlapping) return;
            
            //unghost on click
            if (Input.GetMouseButtonDown(0))
            {
                var roomId = RoomId.GetRoomId(hit.collider.transform);
                var placedNodes = GetGhostOccupiedCells();
                PlaceExhibit(roomId, placedNodes);
            }
        }

        private void PlaceExhibit(string roomId, Vector2Int[] placedNodes)
        {
            Debug.Log("Placing Exhibit in Room " + roomId + " in " + string.Join(", ", placedNodes.Select(x => x.ToString())));
            currentState = PlacementState.NoTarget;
            var inst= Instantiate(exhibitTileType.ExhibitPrefab, ghostTile.transform.position, ghostTile.transform.rotation);
            inst.transform.SetParent(grid.transform);
            CurrentGameState.UpdatePlacedExhibit(exhibitTileType, roomId, placedNodes);
            Message.Publish(new ExhibitPlaced(inst, exhibitTileType));
            DisableGhostObject();
            StopPlacing();
        }

        private Vector2Int[] GetGhostOccupiedCells()
        {
            if (ghostTile == null || !ghostTile.gameObject.activeInHierarchy)
            {
                Debug.Log("GetGhostOccupiedCells: Ghost tile is null or inactive");
                return new Vector2Int[0];
            }

            // Get the ghost tile's child object (the actual ghost exhibit)
            var ghostExhibit = ghostTile.transform.GetChild(0);
            if (ghostExhibit == null) 
            {
                Debug.Log("GetGhostOccupiedCells: No child found in ghost tile");
                return new Vector2Int[0];
            }

            if(debug)
                Debug.Log($"GetGhostOccupiedCells: Ghost exhibit position: {ghostExhibit.position}, scale: {ghostExhibit.localScale}");
            var result = GetGhostOccupiedCellsInternal(ghostExhibit);
            if(debug)
                Debug.Log($"GetGhostOccupiedCells: Found {result.Length} occupied cells: {string.Join(", ", result.Select(x => x.ToString()))}");
            return result;
        }

        private Vector2Int[] GetGhostOccupiedCellsInternal(Transform exhibitTransform)
        {
            var occupiedCells = new HashSet<Vector2Int>();
            var colliders = exhibitTransform.GetComponentsInChildren<Collider>();
            
            Debug.Log($"GetGhostOccupiedCellsInternal: Found {colliders.Length} colliders on {exhibitTransform.name}");
            
            foreach (var col in colliders)
            {
                Debug.Log($"Ghost Collider: {col.name}, isTrigger: {col.isTrigger}, enabled: {col.enabled}");
                
                // For ghost tiles, we include ALL colliders (including triggers) to determine occupied cells
                var bounds = col.bounds;
                var minCell = grid.WorldToCell(bounds.min);
                var maxCell = grid.WorldToCell(bounds.max);
                
                Debug.Log($"Ghost Collider bounds: min={bounds.min}, max={bounds.max}");
                Debug.Log($"Ghost Grid cells: minCell={minCell}, maxCell={maxCell}");
                
                // Add all grid cells that the collider occupies
                for (var x = minCell.x; x <= maxCell.x; x++)
                {
                    for (var z = minCell.z; z <= maxCell.z; z++)
                    {
                        var cellCenter = grid.CellToWorld(new Vector3Int(x, 0, z)) + grid.cellSize * 0.5f;
                        if (bounds.Contains(cellCenter))
                        {
                            Debug.Log($"Ghost Cell {x},{z} center {cellCenter} is within bounds");
                            occupiedCells.Add(new Vector2Int(x, z));
                        }
                        else
                        {
                            Debug.Log($"Ghost Cell {x},{z} center {cellCenter} is NOT within bounds");
                        }
                    }
                }
            }
            
            var result = occupiedCells.ToArray();
            Debug.Log("Ghost Occupied Cells: " + string.Join(", ", result.Select(x => x.ToString())));
            return result;
        }

        private void EnableGhostObject()
        {
            ghostTile.EnablePlaceable();
        }

        private void DisableGhostObject()
        {
            ghostTile.DisablePlaceable();
        }

        protected override void Execute(StartPlacement msg)
        {
            exhibitTileType = msg.exhibit;
            StartPlacing();
        }

        protected override void Execute(StopPlacement msg)
        {
            StopPlacing();
        }
    }
}
