using UnityEngine;

public class PlaySoundRequested 
{
    public SoundType soundType;
    public Vector3 position;
    public RectTransform uiRect;
    public Camera uiCamera;

    public PlaySoundRequested(SoundType soundType, Vector3 position)
    {
        this.soundType = soundType;
        this.position = position;
    }

    public PlaySoundRequested(SoundType soundType, RectTransform uiRect, Camera uiCamera = null)
    {
        this.soundType = soundType;
        this.uiRect = uiRect;
        this.uiCamera = uiCamera;
        this.position = default;
    }
}

