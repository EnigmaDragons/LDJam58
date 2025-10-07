using System.Linq;
using UnityEngine;

public class TilePlacementSystem2 : OnMessage<StartPlacement>
{
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private GhostTile2 ghostTile;
    
    private Camera _camera;
    private ExhibitTileType _exhibit;
    private RoomTargettingCollider _target;
    private bool _valid;
    private bool _outOfBounds = true;
    
    private void Update()
    {
        if (!CurrentGameState.ReadOnly.isPlacing || _exhibit == null)
            return;

        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
        {
            ghostTile.transform.rotation *= Quaternion.Euler(0f, -90f, 0f);
            _target = null;
            Message.Publish(new RotationChanged(-90f));
            return;
        }
        
        if(_camera == null) 
            _camera = Camera.main;
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayerMask))
        {
            var newTarget = hit.collider.GetComponent<RoomTargettingCollider>();
            if (_target != newTarget)
            {
                _outOfBounds = false;
                _target = newTarget;
                _target = hit.collider.GetComponent<RoomTargettingCollider>();
                var room = CurrentGameState.ReadOnly.Rooms[_target.RoomId];
                if (CurrentGameState.ReadOnly.focusedRoom != _target.RoomId)
                    CurrentGameState.UpdateState(x => x.focusedRoom = _target.RoomId);
                if (_target.Nodes.All(x => string.IsNullOrEmpty(room.exhibitIds[x]) || room.exhibitIds[x] == _exhibit.DisplayName))
                {
                    ghostTile.Valid();
                    _valid = true;
                    ghostTile.transform.position = _target.Center;
                    CurrentGameState.GetGhostExhibitEnjoymentScore(_exhibit, _target.RoomId, _target.Nodes);
                }
                else
                {
                    if (_valid)
                        CurrentGameState.InvalidGhostPlacement(_exhibit);
                    ghostTile.Invalid();
                    _valid = false;
                    ghostTile.transform.position = _target.Center;
                }
            }
        }
        else
        {
            if (!_outOfBounds)
                CurrentGameState.InvalidGhostPlacement(_exhibit);
            _outOfBounds = true;
            _valid = false;
            _target = null;
            ghostTile.OutOfBounds();
            ghostTile.transform.position = Input.mousePosition; //TODO
        }

        if (Input.GetMouseButtonDown(0) && _valid)
        {
            var inst = Instantiate(_exhibit.ExhibitPrefab, ghostTile.transform.position, ghostTile.transform.rotation);
            inst.GetComponent<ExhibitPrefab>().Init(_exhibit);
            inst.transform.SetParent(transform);
            CurrentGameState.UpdatePlacedExhibit(_exhibit, _target.RoomId, _target.Nodes, false);
            ghostTile.CleanUp();
            CurrentGameState.UpdateState(state =>
            {
                state.isPlacing = false;
            });
            _exhibit = null;
            Message.Publish(new ExhibitPlaced(inst, _exhibit));
            Message.Publish(new StopPlacement());
        }
    }

    protected override void Execute(StartPlacement msg)
    {
        _exhibit = msg.exhibit;
        CurrentGameState.InvalidGhostPlacement(_exhibit);
        CurrentGameState.UpdateState(gs =>
        {
            gs.isPlacing = true;
            gs.focusedExhibit = msg.exhibit.DisplayName;
        });
        ghostTile.Init(msg.exhibit);
    }
}