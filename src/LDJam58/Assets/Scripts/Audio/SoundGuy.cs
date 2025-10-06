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

public enum SoundType 
{
    None = 0,
    ExhibitPickingBegan = 1,
    ExhibitShownCommon = 2,
    ExhibitShownRare = 3,
    ExhibitShownExotic = 4,
    ExhibitShownMythic = 5,
    ExhibitPicked = 6,
} 

public class SoundGuy : MonoBehaviour 
{
    [SerializeField] private UiSfxPlayer player;
    [SerializeField] private AudioClipVolume exhibitPlacedSound;
    [SerializeField] private AudioClipVolume exhibitPickedSound;

    [SerializeField] private AudioClipVolume exhibitPickingBeganSound;
    [SerializeField] private AudioClipVolume exhibitShownCommonSound;
    [SerializeField] private AudioClipVolume exhibitShownRareSound;
    [SerializeField] private AudioClipVolume exhibitShownExoticSound;
    [SerializeField] private AudioClipVolume exhibitShownMythicSound;



    private void OnEnable()
    {
        Message.Subscribe<ExhibitPlaced>(OnExhibitPlaced, this);
        Message.Subscribe<ExhibitPicked>(OnExhibitPicked, this);
        Message.Subscribe<PlaySoundRequested>(OnPlaySoundRequested, this);
    }

    private void OnDisable()
    {
        Message.Unsubscribe(this);
    }

    private void OnExhibitPicked(ExhibitPicked obj)
    {
        player.Play(exhibitPickedSound);
    }

    private void OnExhibitPlaced(ExhibitPlaced obj)
    {
        player.Play(exhibitPlacedSound);
    }    

    private void OnPlaySoundRequested(PlaySoundRequested obj)
    {
        var sound = GetSound(obj.soundType);
        if (sound == null){
            Debug.LogError($"Sound {obj.soundType} not found");
            return;
        }
        if (obj.uiRect != null)
        {
            player.PlayAtUIRect(sound, obj.uiRect, obj.uiCamera);
        }
        else
        {
            player.Play(sound, obj.position);
        }
    }

    private AudioClipVolume GetSound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.ExhibitPickingBegan: return exhibitPickingBeganSound;
            case SoundType.ExhibitShownCommon: return exhibitShownCommonSound;
            case SoundType.ExhibitShownRare: return exhibitShownRareSound;
            case SoundType.ExhibitShownExotic: return exhibitShownExoticSound;
            case SoundType.ExhibitShownMythic: return exhibitShownMythicSound;
            case SoundType.ExhibitPicked: return exhibitPickedSound;
            default: return null;
        }
    }
}
