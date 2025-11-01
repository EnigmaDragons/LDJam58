using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoomUI : OnMessage<GameStateChanged, RoomTransformed, ExhibitPlaced>
{
    [SerializeField] private int amountAwayToBeShown = 3;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform roomPanel;
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private GameObject totalsPanel;
    [SerializeField] private TextMeshProUGUI totalJoy;
    [SerializeField] private TextMeshProUGUI totalTags;
    [SerializeField] private GameObject synergiesPanel;
    [SerializeField] private TextMeshProUGUI synergies;
    [SerializeField] private Vector3 offset;

    private bool _active;
    private bool _showDetails;
    private string _roomId;

    public void Init(string roomId, int height, int width)
    {
        _roomId = roomId;
        _active = false;
        _showDetails = false;
        canvas.worldCamera = Camera.current;
        roomPanel.sizeDelta = new Vector2(41 * height, 41 * width);
        roomPanel.gameObject.SetActive(false);
        roomName.text = CurrentGameState.ReadOnly.Rooms[_roomId].roomType.Name;
        effect.text = "No effect";
        totalsPanel.SetActive(false);
        totalJoy.text = "Joy: 0";
        totalTags.text = "";
        synergiesPanel.SetActive(false);
        synergies.text = "";
    }
    
    public void Update()
    {
        roomPanel.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.TransformPoint(offset));
    }
    
    protected override void Execute(GameStateChanged msg)
    {
        var state = CurrentGameState.ReadOnly;
        
        if (_active && state.focusedRoom != _roomId)
        {
            _active = false;
            roomPanel.gameObject.SetActive(false);
        }
        else if (!_active && state.focusedRoom == _roomId)
        {
            _active = true;
            roomPanel.gameObject.SetActive(true);
        }

        if (!_showDetails && state.showDetails)
        {
            _showDetails = true;
            totalsPanel.SetActive(true);
            if (!CurrentGameState.ReadOnly.Rooms[_roomId].isTransformed)
                synergiesPanel.SetActive(true);
        }
        else if (_showDetails && !state.showDetails)
        {
            _showDetails = false;
            totalsPanel.SetActive(false);
            synergiesPanel.SetActive(false);
        }
    }

    protected override void Execute(RoomTransformed msg)
    {
        if (msg.RoomId == _roomId)
        {
            var roomType = CurrentGameState.ReadOnly.Rooms[_roomId].roomType;
            roomName.text = $"{roomType.Name} ({string.Join(" ", roomType.Requirement.GroupBy(x => x).Select(x => $"{x.Count()}{x.Key.Sprite()}"))})";
            effect.text = $"{roomType.Multiplier}x joy for {roomType.Requirement.Distinct().Sprites()} exhibits{(roomType.GivesAdjacencyBonus ? $" and those tags are treated as positive adjacency" : "")} in this room";
            synergiesPanel.SetActive(false);
            var exhibits = CurrentGameState.ReadOnly.Exhibits.Values.Where(x => x.roomId == _roomId).ToArray();
            totalJoy.text = $"Joy: {exhibits.Sum(x => x.calculatedEnjoyment)}";
        }
    }

    protected override void Execute(ExhibitPlaced msg)
    {
        var exhibits = CurrentGameState.ReadOnly.Exhibits.Values.Where(x => x.roomId == _roomId).ToArray();
        var roomTags = exhibits.SelectMany(x => x.tags).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        totalJoy.text = $"Joy: {exhibits.Sum(x => x.calculatedEnjoyment)}";
        totalTags.text = string.Join(Environment.NewLine, roomTags.Select(x => $"{x.Value}{x.Key.Sprite()}"));
        if (!CurrentGameState.ReadOnly.Rooms[_roomId].isTransformed)
        {
            var closeRooms = new List<(string displayText, int amountAway)>();
            foreach (var room in RoomPool.All)
            {
                var requirements = new List<(ExhibitTag tag, int target, int amount)>();
                foreach (var exhibitTag in room.Requirement.GroupBy(x => x))
                    requirements.Add(new (exhibitTag.Key, exhibitTag.Count(), roomTags.ContainsKey(exhibitTag.Key) ? roomTags[exhibitTag.Key] : 0));
                var display = $"{room.Name} {string.Join(" ", requirements.Select(x => $"{x.amount}/{x.target}{x.tag.Sprite()}"))}";
                var countAway = requirements.Sum(x => Math.Max(0, x.target - x.amount));
                closeRooms.Add(new (display, countAway));
            }
            synergies.text = string.Join(Environment.NewLine, closeRooms
                .Where(x => x.amountAway <= amountAwayToBeShown)
                .OrderBy(x => x.amountAway)
                .Select(x => x.displayText));   
        }
    }
}