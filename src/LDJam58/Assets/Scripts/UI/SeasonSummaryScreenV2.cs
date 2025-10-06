using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonSummaryScreenV2 : OnMessage<SummarizeSeason>
{
    [SerializeField] private GameObject ui;

    [SerializeField] private TextMeshProUGUI seasonTitle;
    [SerializeField] private RectTransform decorLine;

    [SerializeField] private CanvasGroup minRatingWidget;
    [SerializeField] private TextMeshProUGUI targetAppeal;
    [SerializeField] private TextMeshProUGUI totalAppeal;
    [SerializeField] private TextMeshProUGUI exhibitRatings;
    [SerializeField] private TextMeshProUGUI peopleCount;
    [SerializeField] private TextMeshProUGUI groupCount;
    [SerializeField] private Button continueButton;

    private Sequence _sequence;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
    }

    protected override void Execute(SummarizeSeason msg)
    {
        var gameState = CurrentGameState.ReadOnly;
        ui.SetActive(true);
        
        // Kill any existing animations
        _sequence?.Kill(true);
        decorLine?.DOKill();
        seasonTitle?.DOKill();
        minRatingWidget?.DOKill();
        targetAppeal?.DOKill();
        totalAppeal?.DOKill();
        exhibitRatings?.DOKill();
        peopleCount?.DOKill();
        groupCount?.DOKill();
        
        // Initialize animation states
        var lineScale = decorLine.localScale;
        decorLine.localScale = new Vector3(0f, lineScale.y, lineScale.z);
        
        var title = seasonTitle;
        title.SetText("SEASON " + (gameState.currentSeasonIndex + 1) + " COMPLETE");
        var titleScale = title.transform.localScale;
        title.transform.localScale = new Vector3(0f, titleScale.y, titleScale.z);
        
        minRatingWidget.alpha = 0f;
        
        // Store final values for counting animation
        var finalGroupCount = gameState.currentGroups.Count;
        var finalPeopleCount = gameState.currentGroups.Sum(x => x.peopleCount);
        var finalExhibitRatings = gameState.Exhibits.Values.Sum(x => x.calculatedEnjoyment);
        var finalTargetAppeal = gameState.currentTargetAppeal;
        var finalTotalAppeal = gameState.seasonScore;
        
        // Initialize text with starting values
        groupCount.text = "<sprite name=\"Visitors\">";
        peopleCount.text = "<sprite name=\"Visitor\">";
        exhibitRatings.text = "Exhibit <sprite name=\"Joy\">";
        targetAppeal.text = finalTargetAppeal.ToString();
        totalAppeal.text = "Total <sprite name=\"Rating\">";
        
        // Create animation sequence
        var seq = DOTween.Sequence();
        
        // Line animation
        seq.Append(decorLine.DOScaleX(1f, 1f).SetEase(Ease.OutCubic));
        
        // Title animation
        seq.Append(title.transform.DOScaleX(1f, 1f).SetEase(Ease.OutBack));
        
        // Min rating widget fade in
        seq.Append(minRatingWidget.DOFade(1f, 0.5f));
        
        // Progressive value animations with counting up and punch scale
        seq.AppendCallback(() => AnimateValue(groupCount, finalGroupCount, "<sprite name=\"Visitors\"> ", 0.8f));
        seq.AppendInterval(0.3f);
        
        seq.AppendCallback(() => AnimateValue(peopleCount, finalPeopleCount, "<sprite name=\"Visitor\"> ", 0.8f));
        seq.AppendInterval(0.3f);
        
        seq.AppendCallback(() => AnimateValue(exhibitRatings, finalExhibitRatings, "Exhibit <sprite name=\"Joy\"> ", 0.8f));
        seq.AppendInterval(0.3f);
        
        // Skip animating targetAppeal; it's already set
        
        seq.AppendCallback(() => AnimateValue(totalAppeal, finalTotalAppeal, "Total <sprite name=\"Rating\"> ", 0.8f));
        
        _sequence = seq;
    }
    
    private void AnimateValue(TextMeshProUGUI textComponent, int finalValue, string prefix, float duration)
    {
        var startValue = 0;
        DOTween.To(() => startValue, x => {
            startValue = x;
            textComponent.text = prefix + x.ToString();
        }, finalValue, duration)
        .SetEase(Ease.OutCubic)
        .OnComplete(() => {
            // Punch scale effect when animation completes
            textComponent.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 3, 0.5f);
        });
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

    protected override void AfterDisable()
    {
        _sequence?.Kill(true);
        decorLine?.DOKill();
        seasonTitle?.DOKill();
        minRatingWidget?.DOKill();
        targetAppeal?.DOKill();
        totalAppeal?.DOKill();
        exhibitRatings?.DOKill();
        peopleCount?.DOKill();
        groupCount?.DOKill();
    }
}