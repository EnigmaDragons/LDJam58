using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonSummaryScreen : OnMessage<SummarizeSeason>
{
    [SerializeField] private GameObject ui;
    //Top Group
    [SerializeField] private TextMeshProUGUI topGroupQuantity;
    [SerializeField] private TextMeshProUGUI topGroupFascinations;
    [SerializeField] private TextMeshProUGUI topGroupDisinterests;
    [SerializeField] private TextMeshProUGUI topGroupScore;
    //Top Exhibit
    [SerializeField] private TextMeshProUGUI topExhibitName;
    [SerializeField] private TextMeshProUGUI topExhibitTags;
    [SerializeField] private TextMeshProUGUI topExhibitAppeal;
    //Totals
    [SerializeField] private TextMeshProUGUI targetAppeal;
    [SerializeField] private TextMeshProUGUI totalAppeal;
    [SerializeField] private TextMeshProUGUI exhibitRatings;
    [SerializeField] private TextMeshProUGUI peopleCount;
    [SerializeField] private TextMeshProUGUI groupCount;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
    }

    protected override void Execute(SummarizeSeason msg)
    {
        var gameState = CurrentGameState.ReadOnly;
        ui.SetActive(true);
        
        //Top Group
        var topGroup = gameState.currentGroups.OrderByDescending(x => x.SeasonScore).First();
        topGroupQuantity.text = $"<sprite name=\"Visitor\"> {topGroup.peopleCount.ToString()}";
        topGroupFascinations.text = $"Loves: {DisplayTags(topGroup.Fascinations)}";
        topGroupDisinterests.text = $"Hates: {DisplayTags(topGroup.Disinterests)}";
        topGroupScore.text = $"<sprite name=\"Rating\"> {topGroup.SeasonScore.ToString()}" ;
        
        //Top Exhibit
        var topExhibit = gameState.Exhibits.Values.OrderByDescending(x => x.seasonScore).First();
        topExhibitName.text = $"Top Exhibit: {topExhibit.name}";
        topExhibitTags.text = $"{DisplayTags(topExhibit.tags)}";
        topExhibitAppeal.text = $"<sprite name=\"Rating\"> {topExhibit.seasonScore.ToString()}";
        
        //Totals
        groupCount.text = $"<sprite name=\"Visitors\"> {gameState.currentGroups.Count.ToString()}";
        peopleCount.text = $"<sprite name=\"Visitor\"> {gameState.currentGroups.Sum(x => x.peopleCount).ToString()}";
        exhibitRatings.text = $"Exhibit <sprite name=\"Joy\"> {gameState.Exhibits.Values.Sum(x => x.calculatedEnjoyment).ToString()}";
        targetAppeal.text = $"Minimum <sprite name=\"Rating\"> {gameState.currentTargetAppeal.ToString()}";
        totalAppeal.text = $"Total <sprite name=\"Rating\"> {gameState.seasonScore.ToString()}";
    }
    
    private string DisplayTags(IEnumerable<ExhibitTag> tags)
        => string.Join(" ", tags.Select(x => "<sprite name=\"" + x + "\">"));

    private void Continue()
    {
        ui.SetActive(false);
        if (CurrentGameState.ReadOnly.seasonScore >= CurrentGameState.ReadOnly.currentTargetAppeal)
            Message.Publish(new AdvancePeriod());
        else
            Message.Publish(new GameLost());
    }
}