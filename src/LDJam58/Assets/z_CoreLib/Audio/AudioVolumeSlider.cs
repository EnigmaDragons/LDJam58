using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioChannel channel = AudioChannel.Music;
    [SerializeField] private Slider slider;
    [SerializeField] private bool playPreviewOnPointerUp = true;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (slider == null)
            return;

        if (AudioSystem.Instance != null)
            slider.value = AudioSystem.Instance.GetChannelVolume(channel);

        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        if (AudioSystem.Instance == null)
            return;

        AudioSystem.Instance.SetChannelVolume(channel, value);
    }

    // Hook this up from an EventTrigger or Slider's OnPointerUp event
    public void OnPointerUp()
    {
        if (!playPreviewOnPointerUp || AudioSystem.Instance == null)
            return;

        AudioSystem.Instance.PlayVolumePreview(channel);
    }
}


