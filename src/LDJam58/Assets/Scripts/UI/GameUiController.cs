using Game.Messages;
using UnityEngine;

public class GameUiController : MonoBehaviour
{
    [SerializeField] private GameObject addExhibitButton;
    [SerializeField] private GameObject openMuseumButton;

    private void OnEnable()
    {
        CurrentGameState.Subscribe(OnGameStateChanged, this);
        Message.Subscribe<StartPlacement>(OnStartPlacement, this);
        Message.Subscribe<StopPlacement>(OnStopPlacement, this);
        Message.Subscribe<ExhibitPlaced>(OnExhibitPlaced, this);
    }

    private void OnDisable()
    {
        CurrentGameState.Unsubscribe(this);
        Message.Unsubscribe(this);
    }

    private void Start()
    {
        UpdateButtonVisibility();
    }

    private void OnGameStateChanged(GameStateChanged msg)
    {
        UpdateButtonVisibility();
    }

    private void OnStartPlacement(StartPlacement msg)
    {
        UpdateButtonVisibility();
    }

    private void OnStopPlacement(StopPlacement msg)
    {
        UpdateButtonVisibility();
    }

    private void OnExhibitPlaced(ExhibitPlaced msg)
    {
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        var gameState = CurrentGameState.ReadOnly;
        var exhibitsRemaining = gameState.currentNumExhibitsToPickThisPeriod;
        var isPlacing = gameState.isPlacing;

        // Add Exhibit Button: Show when not placing AND when there are exhibits left to place
        var showAddExhibitButton = !isPlacing && exhibitsRemaining > 0;
        if (addExhibitButton != null)
            addExhibitButton.SetActive(showAddExhibitButton);

        // Open Museum Button: Show when not placing AND when there are 0 exhibits left
        var showOpenMuseumButton = !isPlacing && exhibitsRemaining == 0;
        if (openMuseumButton != null)
            openMuseumButton.SetActive(showOpenMuseumButton);
    }
}
