using UnityEngine;

[CreateAssetMenu]
public sealed class UiSfxPlayer : ScriptableObject
{
    [SerializeField] private AudioSource source;

    public void Init(AudioSource src) => source = src;
    public void InitIfNeeded(AudioSource src) => source.IfNull(() => Init(src));
    public void Play(AudioClipVolume c, Vector3 position = default) => Play(c.clip, c.volume, position);
    public void Play(AudioClip c, float volume = 1f, Vector3 position = default)
    {
        if (source == null)
        {
            Debug.LogError("AudioSource is not initialized");
            return;
        }

        if (c == null)
        {
            Debug.LogError("AudioClip is null");
            return;
        }

        Debug.Log($"Playing {c.name} at {position} with volume {volume}");
        if (position != default)
        {
            AudioSource.PlayClipAtPoint(c, position, volume);
        }
        else
        {
            source.PlayOneShot(c, volume);
        }
    }

    public void PlayAtUIRect(AudioClipVolume c, RectTransform ui, Camera camera = null)
    {
        PlayAtUIRect(c.clip, c.volume, ui, camera);
    }

    public void PlayAtUIRect(AudioClip c, float volume, RectTransform ui, Camera camera = null)
    {
        if (source == null)
        {
            Debug.LogError("AudioSource is not initialized");
            return;
        }

        if (ui == null)
        {
            source.PlayOneShot(c, volume);
            return;
        }

        var cam = camera != null ? camera : Camera.main;

        // Get rect center in world space then convert to screen
        var worldCenter = ui.TransformPoint(ui.rect.center);
        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

        float viewportX;
        if (cam != null)
        {
            var vp = cam.ScreenToViewportPoint(screenPos);
            viewportX = vp.x;
        }
        else
        {
            viewportX = Screen.width > 0 ? Mathf.Clamp01(screenPos.x / Screen.width) : 0.5f;
        }

        var pan = Mathf.Clamp((viewportX * 2f) - 1f, -1f, 1f);

        var prevPan = source.panStereo;
        var prevSpatial = source.spatialBlend;
        source.spatialBlend = 0f;
        source.panStereo = pan;
        source.PlayOneShot(c, volume);
        source.panStereo = prevPan;
        source.spatialBlend = prevSpatial;
    }
}
