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
    
    protected override void Execute(SummarizeSeason msg)
    {
        var gameState = CurrentGameState.ReadOnly;
        ui.SetActive(true);
        
        //Top Group
        var topGroup = gameState.currentGroups.OrderBy(x => x.seasonScore).First();
        topGroupQuantity.text = topGroup.peopleCount.ToString();
        topGroupFascinations.text = DisplayTags(topGroup.Fascinations);
        topGroupDisinterests.text = DisplayTags(topGroup.Disinterests);
        topGroupScore.text = topGroup.seasonScore.ToString();
        
        //Top Exhibit
        var topExhibit = gameState.Exhibits.Values.OrderBy(x => x.seasonScore).First();
        topExhibitName.text = topExhibit.name;
        topExhibitTags.text = DisplayTags(topExhibit.tags);
        topExhibitAppeal.text = topExhibit.seasonScore.ToString();
        
        //Totals
        targetAppeal.text = gameState.currentTargetAppeal.ToString();
        totalAppeal.text = gameState.seasonScore.ToString();
        exhibitRatings.text = gameState.Exhibits.Values.Sum(x => x.calculatedEnjoyment).ToString();
        peopleCount.text = gameState.currentGroups.Sum(x => x.peopleCount).ToString();
        groupCount.text = gameState.currentGroups.Count.ToString();
    }
    
    private string DisplayTags(IEnumerable<ExhibitTag> tags)
        => string.Join(", ", tags.Select(x => x.UserFriendlyText()));
}