using UnityEngine;
using TMPro;

// TODO: Juice the Change!
public class CurrentAppealCounter : MonoBehaviour
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
        UpdateCounter(CurrentGameState.ReadOnly.currentAppeal);
    }

    private void OnGameStateChanged(GameStateChanged msg)
    {
        UpdateCounter(msg.State.currentAppeal);
    }

    private void UpdateCounter(int newValue)
    {
        _counterText.text = newValue.ToString();
    }
}

