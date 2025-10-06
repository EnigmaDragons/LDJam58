
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SeasonIntroductionUi : OnMessage<SeasonInitialized>
{
    [SerializeField] private CanvasGroup _mainGroup;
    [SerializeField] private TextMeshProUGUI _seasonTitle;
    [SerializeField] private RectTransform _decorLine;

    [SerializeField] private CanvasGroup _targetAppealGroup;
    [SerializeField] private TextMeshProUGUI _targetAppealText;
    [SerializeField] private CanvasGroup _numVisitingGroupsGroup;
    [SerializeField] private TextMeshProUGUI _numVisitingGroupsText;

    [SerializeField] private CanvasGroup _numExhibitsToPickGroup;
    [SerializeField] private TextMeshProUGUI _numExhibitsToPickGroupText;

    private Sequence _sequence;

    protected override void Execute(SeasonInitialized msg)
    {
        Init(msg.Period);
    }

    public void Init(ProgressionPeriodConfig period)
    {
        _sequence?.Kill(true);

        _decorLine.DOKill();
        _seasonTitle.DOKill();
        _targetAppealGroup.DOKill();
        _numVisitingGroupsGroup.DOKill();

        var lineScale = _decorLine.localScale;
        _decorLine.localScale = new Vector3(0f, lineScale.y, lineScale.z);

        var title = _seasonTitle;
        title.SetText("SEASON " + (CurrentGameState.ReadOnly.currentSeasonIndex + 1));
        var titleScale = title.transform.localScale;
        title.transform.localScale = new Vector3(0f, titleScale.y, titleScale.z);
        Message.Publish(new LockCameraMovement());

        _targetAppealGroup.alpha = 0f;
        _numVisitingGroupsGroup.alpha = 0f;
        _numExhibitsToPickGroup.alpha = 0f;

        _numExhibitsToPickGroupText.text = period.NumExhibitsToPick.ToString();
        _numVisitingGroupsText.text = period.NumVisitingGroups.ToString();
        _targetAppealText.text = period.TargetAppeal.ToString();

        var seq = DOTween.Sequence();
        seq.Append(_decorLine.DOScaleX(1f, 1f).SetEase(Ease.OutCubic));
        seq.Append(title.transform.DOScaleX(1f, 1f).SetEase(Ease.OutBack));
        seq.Append(_numExhibitsToPickGroup.DOFade(1f, 1f));
        seq.Append(_targetAppealGroup.DOFade(1f, 1f));
        seq.Append(_numVisitingGroupsGroup.DOFade(1f, 1f));
        seq.OnComplete(() => {
            Message.Publish(new UiFadeInRequested());
            _mainGroup.DOFade(0f, 2f);
            Message.Publish(new UnlockCameraMovement());
        });

        _sequence = seq;
    }

    protected override void AfterDisable()
    {
        _sequence?.Kill(true);
        _decorLine?.DOKill();
        _seasonTitle?.DOKill();
        _numExhibitsToPickGroup?.DOKill();
        _targetAppealGroup?.DOKill();
        _numVisitingGroupsGroup?.DOKill();
    }
}
