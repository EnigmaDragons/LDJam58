using UnityEngine;
using TMPro;

// TODO: Juice the Change!
public class CurrentGroupCounter : MonoBehaviour
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
        UpdateCounter(CurrentGameState.ReadOnly.currentGroups.Count);
    }

    private void OnGameStateChanged(GameStateChanged msg)
    {
        UpdateCounter(msg.State.currentGroups.Count);
    }

    private void UpdateCounter(int newValue)
    {
        _counterText.text = newValue.ToString();
    }
}

