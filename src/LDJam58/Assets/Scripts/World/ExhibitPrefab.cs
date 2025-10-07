using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ExhibitPrefab : OnMessage<GameStateChanged>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI exhibitNameLabel;
    [SerializeField] private TextMeshProUGUI joyLabel;
    [SerializeField] private TextMeshProUGUI tagsLabel;
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 offset;
    
    private ExhibitTileType exhibitTileType;

    public ExhibitTileType ExhibitTileType => exhibitTileType;
    
    private void Awake()
    {
        SetDisplay(string.Empty, string.Empty, string.Empty);
    }

    public void Init(ExhibitTileType exhibit)
    {
        canvas.worldCamera = Camera.current;
        exhibitTileType = exhibit;
    }

    public void Update()
    {
        panel.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.TransformPoint(offset));
    }

    private void SetDisplay(string exhibitName, string joy, string tags)
    {
        SetLabel(exhibitNameLabel, exhibitName);
        SetLabel(joyLabel, joy);
        SetLabel(tagsLabel, tags);
    }

    //performance
    private void SetLabel(TextMeshProUGUI label, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            if (label.gameObject.activeSelf)
                label.gameObject.SetActive(false);
        }
        else
        {
            if (!label.gameObject.activeSelf)
                label.gameObject.SetActive(true);
            if (label.text != text)
                label.text = text;
        }
    }

    private void UpdateDisplay()
    {
        var state = CurrentGameState.ReadOnly;
        if (state == null || state.Exhibits == null)
            return;

        if (!state.Exhibits.ContainsKey(exhibitTileType.DisplayName))
            return;
        var exhibit = state.Exhibits[exhibitTileType.DisplayName];

        if (state.isPicking)
        {
            SetDisplay(string.Empty, string.Empty, string.Empty);
        }
        else if (state.isPlacing)
        {
            //invalid ghost placement
            if (exhibit.isGhost)
            {
                if (string.IsNullOrEmpty(exhibit.roomId))
                    SetDisplay(string.Empty, Neutral(exhibitTileType.Enjoyment.ToString()), exhibitTileType.Tags.Sprites());
                else
                {
                    var joy = Neutral(exhibit.baseEnjoyment.ToString());
                    if (exhibit.calculatedEnjoyment > exhibit.baseEnjoyment)
                        joy += Positive($"+{exhibit.calculatedEnjoyment-exhibit.baseEnjoyment}");
                    else if (exhibit.calculatedEnjoyment < exhibit.baseEnjoyment)
                        joy += Negative($"-{exhibit.baseEnjoyment-exhibit.calculatedEnjoyment}");
                    SetDisplay(string.Empty, joy, exhibitTileType.Tags.Sprites());
                }
            }
            else if (exhibit.roomId == state.focusedRoom && !string.IsNullOrEmpty(state.focusedExhibit))
            {
                var ghostExhibit = state.Exhibits[state.focusedExhibit];
                var synergies = CurrentGameState.CalculateAdjacencyBonus(ghostExhibit.tags, exhibit.tags);
                var tags = "";
                var joy = Neutral(exhibit.calculatedEnjoyment.ToString());
                if (synergies.Any(x => x.Item2 > 0))
                {
                    tags = synergies.Select(x => x.Item1).Sprites();
                    joy = Positive(exhibit.calculatedEnjoyment.ToString());
                }
                else if (synergies.Any(x => x.Item2 < 0))
                {
                    tags = synergies.Select(x => x.Item1).Sprites();
                    joy = Negative(exhibit.calculatedEnjoyment.ToString());
                }
                if (exhibit.ghostEnjoyment > exhibit.calculatedEnjoyment)
                    joy += Positive($"+{exhibit.ghostEnjoyment-exhibit.calculatedEnjoyment}");
                else if (exhibit.ghostEnjoyment < exhibit.calculatedEnjoyment)
                    joy += Negative($"-{exhibit.calculatedEnjoyment-exhibit.ghostEnjoyment}");
                SetDisplay(string.Empty, joy, tags);   
            }
            else
            {
                SetDisplay(string.Empty, string.Empty, string.Empty);
            }
        }
        else if (state.isShowingMuseum)
        {
            SetDisplay(string.Empty, string.Empty, string.Empty);
        }
        else
        {
            //hovering
            if (state.focusedExhibit == exhibit.name)
                SetDisplay(exhibit.name, Neutral(exhibit.calculatedEnjoyment.ToString()), exhibit.tags.Sprites());
            //hovering room & show details
            else if (state.showDetails && state.focusedRoom == exhibit.roomId)
            {
                SetDisplay("", Neutral(exhibit.calculatedEnjoyment.ToString()), exhibit.tags.Sprites());
            }
            else 
                SetDisplay(string.Empty, string.Empty, string.Empty);
        }
    }

    private string Neutral(string strToWrap)
        => $"<color=black>{strToWrap}</color>";
    
    private string Positive(string strToWrap)
        => $"<color=green>{strToWrap}</color>";

    private string Negative(string strToWrap)
        => $"<color=red>{strToWrap}</color>";

    protected override void Execute(GameStateChanged msg)
    {
        UpdateDisplay();
    }
}