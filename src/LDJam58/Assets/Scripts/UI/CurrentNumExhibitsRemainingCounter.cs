using UnityEngine;
using TMPro;

// TODO: Juice the Change!
public class CurrentNumExhibitsRemainingCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;

    private void OnEnable()
    {
        CurrentGameState.Subscribe(OnGameStateChanged, this);
    }

    private void OnDisable()
    {
        CurrentGameState.Unsubscribe(this);
    }

    private void Start()
    {
        UpdateCounter(CurrentGameState.ReadOnly.currentNumExhibitsToPickThisPeriod);
    }

    private void OnGameStateChanged(GameStateChanged msg)
    {
        UpdateCounter(msg.State.currentNumExhibitsToPickThisPeriod);
    }

    private void UpdateCounter(int newValue)
    {
        _counterText.text = newValue.ToString();
    }
}

