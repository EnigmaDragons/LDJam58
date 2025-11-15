using E7.Introloop;
using UnityEngine;

public class RequestMusicOnEnable : MonoBehaviour
{
    [SerializeField] private IntroloopAudio music;
    [SerializeField] private int musicIndex = -1;
    [SerializeField] private bool useIndex = false;

    private void OnEnable()
    {
        if (useIndex && musicIndex >= 0)
        {
            Message.Publish(new PlayMusicRequested(musicIndex));
        }
        else if (music != null)
        {
            Message.Publish(new PlayMusicRequested(music));
        }
    }
}

