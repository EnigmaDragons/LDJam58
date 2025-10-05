using E7.Introloop;
using UnityEngine;

public class IntroLoopMusicPlaylist : MonoBehaviour
{
    [SerializeField] private IntroloopAudio[] musics;
    [SerializeField] private IntroLoopAudioPlayer musicPlayer;

    private int _currentMusicIndex = 0;

    private void Start()
    {
        musicPlayer.PlaySelectedMusicLooping(musics[_currentMusicIndex % musics.Length]);
    }

    public void PlayMusic(int index)
    {
        _currentMusicIndex = index % musics.Length;
        musicPlayer.PlaySelectedMusicLooping(musics[_currentMusicIndex]);
    }
}
