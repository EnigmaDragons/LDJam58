using Game.Messages;
using UnityEngine;

[CreateAssetMenu(fileName = "EventPublisher", menuName = "EventPublisher")]
public class EventPublisher : ScriptableObject
{
    public static void FadeInScene() => Message.Publish(new UiFadeInRequested());
    public static void FadeOutScene() => Message.Publish(new UiFadeOutRequested());

    public static void StartExhibitPick() => Message.Publish(new StartExhibitPick());
    public static void OpenMuseum() => Message.Publish(new OpenMuseum());

    public static void SeasonInitializedCheat() 
    {
        var gs = CurrentGameState.ReadOnly;
        Message.Publish(new SeasonInitialized(gs.progressionConfig.GetPeriod(gs.currentSeasonIndex)));
    }

    public static void PublishGameLost() => Message.Publish(new GameLost());

    public static void PublishGameWon() => Message.Publish(new GameWon());

    public static void ShowGameGuide() => Message.Publish(new ShowGameGuideRequested());

}
