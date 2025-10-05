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
        var topGroup = gameState.currentGroups.OrderByDescending(x => x.seasonScore).First();
        topGroupQuantity.text = $"Visitors: {topGroup.peopleCount.ToString()}";
        topGroupFascinations.text = $"Fascinations: {DisplayTags(topGroup.Fascinations)}";
        topGroupDisinterests.text = $"Disinterests: {DisplayTags(topGroup.Disinterests)}";
        topGroupScore.text = $"Total Appeal: {topGroup.seasonScore.ToString()}" ;
        
        //Top Exhibit
        var topExhibit = gameState.Exhibits.Values.OrderByDescending(x => x.seasonScore).First();
        topExhibitName.text = $"Top Exhibit: {topExhibit.name}";
        topExhibitTags.text = $"{DisplayTags(topExhibit.tags)}";
        topExhibitAppeal.text = $"Appeal: {topExhibit.seasonScore.ToString()}";
        
        //Totals
        groupCount.text = $"Groups: {gameState.currentGroups.Count.ToString()}";
        peopleCount.text = $"Visitors: {gameState.currentGroups.Sum(x => x.peopleCount).ToString()}";
        exhibitRatings.text = $"Total Exhibit Appeal: {gameState.Exhibits.Values.Sum(x => x.calculatedEnjoyment).ToString()}";
        targetAppeal.text = $"Required Appeal: {gameState.currentTargetAppeal.ToString()}";
        totalAppeal.text = $"Total Appeal: {gameState.seasonScore.ToString()}";
    }
    
    private string DisplayTags(IEnumerable<ExhibitTag> tags)
        => string.Join(", ", tags.Select(x => x.UserFriendlyText()));

    private void Continue()
    {
        ui.SetActive(false);
        if (CurrentGameState.ReadOnly.seasonScore >= CurrentGameState.ReadOnly.currentTargetAppeal)
            Message.Publish(new AdvancePeriod());
        else
            Message.Publish(new GameLost());
    }
}