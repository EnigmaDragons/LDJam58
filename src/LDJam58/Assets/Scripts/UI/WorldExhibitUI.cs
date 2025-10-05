using TMPro;
using UnityEngine;

public class WorldExhibitUI : OnMessage<ScoresUpdated>
{
    [SerializeField] private TextMeshProUGUI exhibitNameLabel;
    [SerializeField] private TextMeshProUGUI enjoyment;

    private string _exhibitName;
    private int _currentScore = 0;
    
    public void Init(ExhibitTileType exhibit)
    {
        _exhibitName = exhibit.DisplayName;
        exhibitNameLabel.text = exhibit.DisplayName;
        _currentScore = exhibit.Enjoyment;
        enjoyment.text = _currentScore.ToString();
    }

    protected override void Execute(ScoresUpdated msg)
        => UpdateScore();

    private void UpdateScore()
    {
        var newScore = CurrentGameState.ReadOnly.Exhibits[_exhibitName].calculatedEnjoyment;
        if (newScore == _currentScore)
            return;
        _currentScore = newScore;
        enjoyment.text = _currentScore.ToString();
    }
}