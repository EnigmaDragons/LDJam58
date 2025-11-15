using E7.Introloop;
using UnityEngine;

public class PlayMusicOnEnable : MonoBehaviour
{
    [SerializeField] private IntroloopAudio music;

    private void OnEnable()
    {
        if (music == null || AudioSystem.Instance == null)
            return;

        AudioSystem.Instance.PlayMusic(music);
    }
}

