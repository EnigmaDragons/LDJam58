public class PlaySeasonMusic : OnMessage<SeasonInitialized>
{
    protected override void Execute(SeasonInitialized msg)
    {
        Message.Publish(new PlayMusicRequested(CurrentGameState.ReadOnly.currentSeasonIndex));
    }
}
