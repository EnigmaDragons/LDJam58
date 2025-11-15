using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlaySoundOnClick : MonoBehaviour
{
    [SerializeField] private SoundType sound = SoundType.UIButtonClickPrimary;
    [SerializeField] private bool useUiRectPanning = true;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (AudioSystem.Instance == null)
            return;

        if (useUiRectPanning)
        {
            var rect = transform as RectTransform;
            AudioSystem.Instance.PlayAtUIRect(sound, rect);
        }
        else
        {
            AudioSystem.Instance.Play(sound, default);
        }
    }
}


