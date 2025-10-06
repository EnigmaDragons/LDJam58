using UnityEngine;

public class SoundGuy : MonoBehaviour 
{
    [SerializeField] private UiSfxPlayer player;
    [SerializeField] private AudioClipVolume exhibitPlacedSound;
    [SerializeField] private AudioClipVolume exhibitPickedSound;


    private void OnEnable()
    {
        Message.Subscribe<ExhibitPlaced>(OnExhibitPlaced, this);
        Message.Subscribe<ExhibitPicked>(OnExhibitPicked, this);
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
}
