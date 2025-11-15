using E7.Introloop;

public class PlayMusicRequested
{
    public IntroloopAudio Music { get; }
    public int? MusicIndex { get; }

    public PlayMusicRequested(IntroloopAudio music)
    {
        Music = music;
        MusicIndex = null;
    }

    public PlayMusicRequested(int musicIndex)
    {
        Music = null;
        MusicIndex = musicIndex;
    }
}

