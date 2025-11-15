using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSystem : MonoBehaviour
{
    private const string MusicVolumePrefsKey = "Audio_MusicVolume";
    private const string SoundVolumePrefsKey = "Audio_SoundVolume";
    private const float DefaultVolume = 0.5f;

    private static AudioSystem _instance;
    public static AudioSystem Instance => _instance;

    [Header("Config")]
    [SerializeField] private AudioConfig config;

    [Header("Runtime Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        var existing = FindFirstObjectByType<AudioSystem>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject("AudioSystem");
        _instance = go.AddComponent<AudioSystem>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (config == null)
            TryLoadConfigFromResources();

        InitSources();
        InitVolumesFromPrefs();

        if (config != null && config.IntroLoopPlayer != null)
            config.IntroLoopPlayer.Init();

        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Handle initial scene load
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        // Subscribe to music and sound requests
        Message.Subscribe<PlayMusicRequested>(OnPlayMusicRequested, this);
        Message.Subscribe<PlaySoundRequested>(OnPlaySoundRequested, this);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Message.Unsubscribe(this);
    }

    private void OnPlayMusicRequested(PlayMusicRequested msg)
    {
        if (msg.Music != null)
        {
            PlayMusic(msg.Music);
        }
        else if (msg.MusicIndex.HasValue)
        {
            PlayMusicByIndex(msg.MusicIndex.Value);
        }
    }

    private void OnPlaySoundRequested(PlaySoundRequested msg)
    {
        if (msg.uiRect != null)
        {
            PlayAtUIRect(msg.soundType, msg.uiRect, msg.uiCamera);
        }
        else
        {
            Play(msg.soundType, msg.position);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (config == null)
            return;

        var music = config.GetMusicForScene(scene.name);
        if (music != null)
        {
            PlayMusic(music);
        }
        // Don't stop music if no scene mapping - let game events handle music
    }

    private void TryLoadConfigFromResources()
    {
        // Convention: Resources/AudioConfig.asset
        config = Resources.Load<AudioConfig>("AudioConfig");
        if (config == null)
            Debug.LogWarning("AudioSystem could not find an AudioConfig in Resources named 'AudioConfig'. Please create one and assign it.");
    }

    private void InitSources()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (soundSource == null)
            soundSource = gameObject.AddComponent<AudioSource>();

        if (config == null)
            return;

        var musicConfig = config.GetChannelConfig(AudioChannel.Music);
        if (musicConfig != null && musicConfig.MixerGroup != null)
            musicSource.outputAudioMixerGroup = musicConfig.MixerGroup;

        var soundConfig = config.GetChannelConfig(AudioChannel.Sound);
        if (soundConfig != null && soundConfig.MixerGroup != null)
            soundSource.outputAudioMixerGroup = soundConfig.MixerGroup;
    }

    private void InitVolumesFromPrefs()
    {
        if (config == null || config.Mixer == null)
            return;

        SetChannelVolume(AudioChannel.Music, PlayerPrefs.GetFloat(MusicVolumePrefsKey, DefaultVolume), false);
        SetChannelVolume(AudioChannel.Sound, PlayerPrefs.GetFloat(SoundVolumePrefsKey, DefaultVolume), false);
    }

    public float GetChannelVolume(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Music:
                return PlayerPrefs.GetFloat(MusicVolumePrefsKey, DefaultVolume);
            case AudioChannel.Sound:
                return PlayerPrefs.GetFloat(SoundVolumePrefsKey, DefaultVolume);
            default:
                return 1f;
        }
    }

    public void SetChannelVolume(AudioChannel channel, float normalizedValue, bool save = true)
    {
        if (config == null || config.Mixer == null)
            return;

        var clamped = Mathf.Clamp(normalizedValue, 0.0001f, 1f);
        var channelConfig = config.GetChannelConfig(channel);
        if (channelConfig == null)
            return;

        var mixerVolume = Mathf.Log10(clamped) * 20f - channelConfig.ReductionDb;
        config.Mixer.SetFloat(channelConfig.MixerParameterName, mixerVolume);

        if (!save)
            return;

        switch (channel)
        {
            case AudioChannel.Music:
                PlayerPrefs.SetFloat(MusicVolumePrefsKey, clamped);
                break;
            case AudioChannel.Sound:
                PlayerPrefs.SetFloat(SoundVolumePrefsKey, clamped);
                break;
        }
    }

    public void Play(SoundType sound, Vector3 position)
    {
        if (config == null)
        {
            Debug.LogWarning("AudioSystem has no AudioConfig; cannot play sound.");
            return;
        }

        var def = config.GetSound(sound);
        if (def == null || def.Clip == null)
        {
            Debug.LogWarning($"AudioSystem could not find sound definition for {sound}.");
            return;
        }

        var clip = def.Clip;
        var volume = def.Volume;

        var src = GetSourceForChannel(AudioChannel.Sound);
        if (src == null)
        {
            Debug.LogWarning("AudioSystem has no AudioSource configured for Sound channel.");
            return;
        }

        // Ensure the source has the correct mixer group
        EnsureSourceHasMixerGroup(src, AudioChannel.Sound);

        // Get the Sound channel mixer group from config
        var soundConfig = config.GetChannelConfig(AudioChannel.Sound);
        var mixerGroup = soundConfig != null ? soundConfig.MixerGroup : null;

        if (position != default)
        {
            // Create a temporary AudioSource at the position with the Sound channel mixer group
            var tempGo = new GameObject("TempAudioSource");
            tempGo.transform.position = position;
            var tempSource = tempGo.AddComponent<AudioSource>();
            
            // Assign mixer group from config
            if (mixerGroup != null)
                tempSource.outputAudioMixerGroup = mixerGroup;
            
            // Configure for 3D spatial audio
            tempSource.spatialBlend = 1f; // Full 3D
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 50f;
            tempSource.spread = 0f;
            
            tempSource.PlayOneShot(clip, volume);
            Destroy(tempGo, clip.length + 0.1f);
            return;
        }

        src.PlayOneShot(clip, volume);
    }

    public void PlayAtUIRect(SoundType sound, RectTransform uiRect, Camera camera = null)
    {
        if (config == null)
        {
            Debug.LogWarning("AudioSystem has no AudioConfig; cannot play UI sound.");
            return;
        }

        var def = config.GetSound(sound);
        if (def == null || def.Clip == null)
        {
            Debug.LogWarning($"AudioSystem could not find sound definition for {sound}.");
            return;
        }

        var src = GetSourceForChannel(AudioChannel.Sound);
        if (src == null)
        {
            Debug.LogWarning("AudioSystem has no AudioSource configured for Sound channel.");
            return;
        }

        // Ensure the source has the correct mixer group
        EnsureSourceHasMixerGroup(src, AudioChannel.Sound);

        PlayClipAtUIRect(src, def.Clip, def.Volume, uiRect, camera);
    }

    private AudioSource GetSourceForChannel(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Music:
                return musicSource;
            case AudioChannel.Sound:
                return soundSource;
            default:
                return soundSource;
        }
    }

    private void EnsureSourceHasMixerGroup(AudioSource source, AudioChannel channel)
    {
        if (source == null || config == null)
            return;

        var channelConfig = config.GetChannelConfig(channel);
        if (channelConfig != null && channelConfig.MixerGroup != null)
        {
            if (source.outputAudioMixerGroup != channelConfig.MixerGroup)
            {
                source.outputAudioMixerGroup = channelConfig.MixerGroup;
                Debug.Log($"AudioSystem: Assigned {channelConfig.MixerGroup.name} mixer group to {channel} channel AudioSource.");
            }
        }
        else
        {
            Debug.LogWarning($"AudioSystem: No mixer group configured for {channel} channel. Sounds may not respect volume settings.");
        }
    }

    private void PlayClipAtUIRect(AudioSource source, AudioClip clip, float volume, RectTransform uiRect, Camera camera = null)
    {
        if (source == null || clip == null)
            return;

        if (uiRect == null)
        {
            source.PlayOneShot(clip, volume);
            return;
        }

        var cam = camera != null ? camera : Camera.main;
        var worldCenter = uiRect.TransformPoint(uiRect.rect.center);
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
        source.PlayOneShot(clip, volume);
        source.panStereo = prevPan;
        source.spatialBlend = prevSpatial;
    }

    public void PlayMusic(E7.Introloop.IntroloopAudio music)
    {
        if (config == null)
        {
            Debug.LogWarning("AudioSystem has no AudioConfig; cannot play music.");
            return;
        }

        if (music == null)
        {
            Debug.LogWarning("AudioSystem cannot play null music.");
            return;
        }

        var player = config.IntroLoopPlayer;
        if (player == null)
        {
            Debug.LogWarning("AudioSystem has no IntroLoopAudioPlayer assigned in AudioConfig.");
            return;
        }

        player.PlaySelectedMusicLooping(music);
    }

    public void PlayMusicByIndex(int index)
    {
        if (config == null)
        {
            Debug.LogWarning("AudioSystem has no AudioConfig; cannot play music.");
            return;
        }

        var player = config.IntroLoopPlayer;
        if (player == null)
        {
            Debug.LogWarning("AudioSystem has no IntroLoopAudioPlayer assigned in AudioConfig.");
            return;
        }

        var track = config.GetMusicTrack(index);
        if (track == null)
        {
            Debug.LogWarning($"AudioSystem could not find music track at index {index}.");
            return;
        }

        player.PlaySelectedMusicLooping(track);
    }

    public void StopMusic()
    {
        if (config == null)
            return;

        try
        {
            var player = E7.Introloop.IntroloopPlayer.Instance;
            if (player != null)
                player.Stop();
        }
        catch
        {
            // If IntroloopPlayer isn't available or Stop() doesn't exist, silently fail
        }
    }

    public void PlayVolumePreview(AudioChannel channel)
    {
        if (config == null)
            return;

        var channelConfig = config.GetChannelConfig(channel);
        if (channelConfig == null || channelConfig.PreviewClip == null)
            return;

        var src = GetSourceForChannel(channel);
        if (src == null)
            return;

        src.PlayOneShot(channelConfig.PreviewClip);
    }
}


