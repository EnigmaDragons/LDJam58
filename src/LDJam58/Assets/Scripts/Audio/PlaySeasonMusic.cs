using UnityEngine;

public class PlaySeasonMusic : OnMessage<SeasonInitialized>
{
    [SerializeField] private IntroLoopMusicPlaylist musicPlayer;

    protected override void Execute(SeasonInitialized msg)
    {
        musicPlayer.PlayMusic(CurrentGameState.ReadOnly.currentSeasonIndex);
    }
}
