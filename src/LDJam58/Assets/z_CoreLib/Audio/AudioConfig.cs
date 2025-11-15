using System;
using E7.Introloop;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Channels")]
    [SerializeField] private ChannelConfig[] channels;

    [Header("Sounds")]
    [SerializeField] private SoundDefinition[] sounds;

    [Header("Music (Introloop)")]
    [SerializeField] private IntroLoopAudioPlayer introLoopPlayer;
    [SerializeField] private IntroloopAudio[] musicTracksByIndex;
    [SerializeField] private SceneMusicMapping[] sceneMusicMappings;

    public AudioMixer Mixer => mixer;
    public IntroLoopAudioPlayer IntroLoopPlayer => introLoopPlayer;

    public ChannelConfig GetChannelConfig(AudioChannel channel)
    {
        if (channels == null)
            return null;

        for (var i = 0; i < channels.Length; i++)
        {
            if (channels[i] != null && channels[i].Channel == channel)
                return channels[i];
        }

        return null;
    }

    public SoundDefinition GetSound(SoundType sound)
    {
        if (sounds == null)
            return null;

        for (var i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] != null && sounds[i].Id == sound)
                return sounds[i];
        }

        return null;
    }

    public IntroloopAudio GetMusicTrack(int index)
    {
        if (musicTracksByIndex == null || musicTracksByIndex.Length == 0)
            return null;

        if (index < 0)
            index = 0;

        if (index >= musicTracksByIndex.Length)
            index = index % musicTracksByIndex.Length;

        return musicTracksByIndex[index];
    }

    public IntroloopAudio GetMusicForScene(string sceneName)
    {
        if (sceneMusicMappings == null || string.IsNullOrEmpty(sceneName))
            return null;

        for (var i = 0; i < sceneMusicMappings.Length; i++)
        {
            if (sceneMusicMappings[i] != null && sceneMusicMappings[i].SceneName == sceneName)
                return sceneMusicMappings[i].Music;
        }

        return null;
    }

    [Serializable]
    public class ChannelConfig
    {
        [SerializeField] private AudioChannel channel;
        [SerializeField] private string mixerParameterName;
        [SerializeField] private FloatReference reductionDb = new FloatReference(0f);
        [SerializeField] private AudioClip previewClip;
        [SerializeField] private AudioMixerGroup mixerGroup;

        public AudioChannel Channel => channel;
        public string MixerParameterName => mixerParameterName;
        public FloatReference ReductionDb => reductionDb;
        public AudioClip PreviewClip => previewClip;
        public AudioMixerGroup MixerGroup => mixerGroup;
    }

    [Serializable]
    public class SoundDefinition
    {
        [SerializeField] private SoundType id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        public SoundType Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;

        public AudioClipVolume AsAudioClipVolume
        {
            get
            {
                var result = new AudioClipVolume();
                result.clip = clip;
                result.volume = volume;
                return result;
            }
        }
    }

    [Serializable]
    public class SceneMusicMapping
    {
        [SerializeField] private string sceneName;
        [SerializeField] private IntroloopAudio music;

        public string SceneName => sceneName;
        public IntroloopAudio Music => music;
    }
}


