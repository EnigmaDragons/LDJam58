using UnityEngine;

public class SeasonIntroductionUiController : OnMessage<SeasonInitialized>

{
    [SerializeField] private SeasonIntroductionUi _seasonIntroductionUiPrefab;

    private SeasonIntroductionUi _seasonIntroductionUi;


    protected override void Execute(SeasonInitialized msg)
    {
        if (_seasonIntroductionUi != null)
            Object.Destroy(_seasonIntroductionUi.gameObject);

        _seasonIntroductionUi = Instantiate(_seasonIntroductionUiPrefab, transform);
        _seasonIntroductionUi.gameObject.SetActive(true);
        _seasonIntroductionUi.Init(msg.Period);
    }   
}
