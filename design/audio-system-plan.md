### Audio System Refactor – Implementation Plan

This plan describes concrete steps to move to a code‑first, centralized audio system while preserving existing features and UI.

---

### Phase 0 – Inventory & Decisions

- **0.1 Music backend choice**
  - Use **Introloop** for all background music in this project.
  - Keep `GameMusicPlayer` only for legacy compatibility until all scenes are migrated, but do not add new usages.

- **0.2 Channels and sounds**
  - Define the canonical set of **audio channels** (enum): `Master`, `Music`, `Sound`.
  - Only `Music` and `Sound` will be user‑controllable; `Master` is mixer‑level only (no UI slider).
  - Confirm and list all gameplay/UI **sound types** currently used:
    - Existing `SoundType` enum entries.
    - Ad‑hoc button sounds via `OnClickPlayUiSound` and `ButtonSound`.
    - Any extra SFX like ambient/random sounds.

- **0.3 Mixer mapping**
  - Use a single main `AudioMixer` asset.
  - Map channels to mixer parameters:
    - `Music` → `"MusicVolume"`.
    - `Sound` → `"SoundVolume"`.
    - `Master` stays at the mixer level and is not exposed to the player.
  - Use per‑channel `FloatReference reductionDb` values as needed for fine‑tuning perceived loudness.

---

### Phase 1 – Core Configuration Types

- **1.1 Introduce `AudioChannel` enum**
  - Create a new enum (no namespace) listing all channels.
  - Ensure it is generic and reusable across code and UI.

- **1.2 Consolidate `SoundType` → `SoundId` (optional rename)**
  - Either:
    - Reuse `SoundType` as the canonical SFX ID enum, or
    - Introduce `SoundId` enum and migrate references gradually.
  - Make sure every sound used anywhere in the project has an enum entry.

- **1.3 Create `AudioConfig` ScriptableObject**
  - New `AudioConfig` asset to hold:
    - Reference to the main `AudioMixer`.
    - A list of **channel configs**:
      - `AudioChannel channel`
      - `string mixerParameterName`
      - `FloatReference reductionDb`
      - Optional preview clip reference per channel.
    - A list of **sound definitions**:
      - `SoundType`/`SoundId` id
      - `AudioChannel channel`
      - `AudioClipVolume` (clip + volume)
    - A list of **music tracks / playlists**:
      - For Introloop: arrays of `IntroloopAudio` with logical names or indexes.
      - Optionally map season index → music track.
  - Implement helper methods on `AudioConfig` for lookups:
    - `GetChannelConfig(AudioChannel channel)`
    - `GetSound(SoundType id)`
    - `GetMusicTrack(int index)` (and/or by id).

---

### Phase 2 – Central `AudioSystem` Singleton

- **2.1 Create `AudioSystem` MonoBehaviour**
  - Responsibilities:
    - Hold a reference to the `AudioConfig` asset.
    - Manage `AudioSource` instances for UI, SFX, music, narrator.
    - Interface with the `AudioMixer`.
    - Own the volume persistence logic (PlayerPrefs).
    - Provide `Play`/`PlayAtUIRect`/music APIs.
  - Implement without namespaces (per project rule).

- **2.2 Code‑first bootstrap**
  - Use `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` to:
    - Create a GameObject (`"AudioSystem"`) if not present.
    - Add `AudioSystem` and `DontDestroyOnLoad`.
  - In `Awake`, ensure singleton semantics (destroy duplicates).
  - Initialize:
    - All `AudioSource` components and assign mixer groups based on `AudioConfig`.
    - Channel volumes (read from PlayerPrefs and call `mixer.SetFloat(...)`).

- **2.3 Playback API**
  - Add methods to `AudioSystem`:
    - `void Play(SoundType id, Vector3 position)`
    - `void PlayAtUIRect(SoundType id, RectTransform uiRect, Camera camera = null)`
    - `void PlayMusic(int index)` (Introloop or standard, depending on Phase 0 decision).
    - Optionally: `void FadeOutMusic(float duration)` using existing `FadeOutAsync`.
  - Internally:
    - Resolve sound definition via `AudioConfig`.
    - Select the correct `AudioSource` for the channel.
    - Apply existing UI panning logic (migrated from `UiSfxPlayer` into `AudioSystem` or a shared helper).

---

### Phase 3 – Volume Management Centralization

- **3.1 Move volume logic into `AudioSystem`**
  - Implement:
    - `float GetChannelVolume(AudioChannel channel)` → returns normalized slider value from PlayerPrefs or default.
    - `void SetChannelVolume(AudioChannel channel, float normalizedValue, bool save = true)`:
      - Look up `ChannelConfig` from `AudioConfig`.
      - Compute dB using existing log10 formula and `reductionDb`.
      - Call `mixer.SetFloat(...)`.
      - Optionally save to PlayerPrefs.
    - `void PlayVolumePreview(AudioChannel channel)`:
      - Pick preview clip from `ChannelConfig`.
      - Route through corresponding `AudioSource` and mixer group.

- **3.2 Replace `MixerVolumeSlider`**
  - Introduce `AudioVolumeSlider` component:
    - Serialized `AudioChannel channel` and `Slider slider`.
    - `Start()`:
      - Set `slider.value` from `AudioSystem.Instance.GetChannelVolume(channel)`.
      - Wire `slider.onValueChanged` → `AudioSystem.Instance.SetChannelVolume(channel, value)`.
    - Optional pointer‑up handler or UnityEvent to call `AudioSystem.Instance.PlayVolumePreview(channel)`.
  - Phase out `MixerVolumeSlider`:
    - Replace usages in scenes with `AudioVolumeSlider`.
    - When all replaced, delete `MixerVolumeSlider` and its scene references.

- **3.3 Fold `InitAudioVolumeLevel` and `OnVolumeChangedSound`**
  - Remove `InitAudioVolumeLevel`:
    - Its logic is now in `AudioSystem.InitVolumesFromPrefs()`.
  - Remove `OnVolumeChangedSound`:
    - Replace preview behavior with `AudioSystem.PlayVolumePreview(channel)` wired from UI events.
  - Clean up any remaining references to `MixerVolumeChanged` messages if no longer needed.

---

### Phase 4 – SFX & UI Sounds Migration

- **4.1 Refactor `UiSfxPlayer` usage**
  - Decide whether to:
    - Keep `UiSfxPlayer` as a thin helper used internally by `AudioSystem`, or
    - Inline its logic directly into `AudioSystem`.
  - Recommended: move the UI‑panning implementation into `AudioSystem` (or a static helper) and:
    - Update callers to use `AudioSystem.Instance.Play(...)` / `PlayAtUIRect(...)`.
    - Mark `UiSfxPlayer` as deprecated or remove it once all references are migrated.

- **4.2 Replace `OnClickPlayUiSound` and `ButtonSound`**
  - Create `PlaySoundOnClick`:
    - `[RequireComponent(typeof(Button))]`.
    - Serialized `SoundType`/`SoundId` and a `bool useUiRectPanning`.
    - On click, call `AudioSystem.Instance.Play(...)` or `PlayAtUIRect(...)`.
  - Update scenes:
    - Replace `OnClickPlayUiSound` and `ButtonSound` instances with `PlaySoundOnClick`.
    - Pick appropriate `SoundType` for each button.
  - After migration, delete old components and unused fields.

- **4.3 Replace `SoundGuy` and `PlaySoundRequested` handling**
  - Introduce a small bridge (if keeping message flow):
    - Component or static handler that subscribes to `PlaySoundRequested` and calls `AudioSystem` directly.
  - Migrate `SoundGuy`:
    - Replace internal serialized `AudioClipVolume` fields with entries in `AudioConfig`.
    - Replace direct `UiSfxPlayer` calls with `AudioSystem.Instance.Play(...)` / `PlayAtUIRect(...)`.
    - Optionally:
      - Phase out `SoundGuy` entirely by having gameplay code publish `PlaySoundRequested` or call `AudioSystem` directly.
  - Ensure `ExhibitPickerView` and `SeasonSummaryScreenV2` now only depend on the message or direct `AudioSystem` calls, not on scene‑local audio wiring.

---

### Phase 5 – Music System Consolidation

- **5.1 Standardize on a music path**
  - If choosing Introloop:
    - Make `AudioSystem` own an `IntroLoopAudioPlayer` reference configured via `AudioConfig`.
    - Expose methods:
      - `PlayMusic(int index)`
      - `PlayMusicById(...)` if you add music IDs.
    - Move persistent initialization of `IntroLoopAudioPlayer` into `AudioSystem` (no separate `InitIntroLoopAudioPlayer` object).
  - If keeping standard `AudioSource` music:
    - Reuse `GameMusicPlayer` logic inside `AudioSystem`:
      - `AudioSystem`’s `musicSource` becomes the main music source.
      - Fade‑out and track switching wrapped by `AudioSystem`.

- **5.2 Migrate `IntroLoopMusicPlaylist` and `PlaySeasonMusic`**
  - Add playlist info into `AudioConfig`:
    - Array of `IntroloopAudio` or structured playlists.
    - Mapping from season index → track index.
  - Adapt `PlaySeasonMusic`:
    - Replace dependency on `IntroLoopMusicPlaylist` with a call to `AudioSystem.Instance.PlayMusic(CurrentGameState.ReadOnly.currentSeasonIndex)`.
  - Remove `IntroLoopMusicPlaylist` after all references are removed.

- **5.3 Migrate `SceneBackgroundMusic`**
  - Replace `SceneBackgroundMusic` usage:
    - For scenes that play a static background loop, call `AudioSystem.Instance.PlayMusic(...)` from a light scene script that only specifies an index or ID, not a clip/mixer/player.
  - Remove `InitGameMusicPlayer` and `SceneBackgroundMusic` once all scenes use `AudioSystem` for music.

---

### Phase 6 – Scene & Prefab Cleanup

- **6.1 Remove legacy init components**
  - Systematically search and delete:
    - `InitUiSfxPlayer` instances and prefab references.
    - `InitGameMusicPlayer` instances.
    - `InitIntroLoopAudioPlayer` instances.
    - `InitAudioVolumeLevel` instances.
  - Verify that `AudioSystem` bootstraps correctly in all entry points (main menu, gameplay, etc.).

- **6.2 Update prefabs and UI**
  - Replace all remaining:
    - `MixerVolumeSlider` → `AudioVolumeSlider`.
    - `OnClickPlayUiSound` / `ButtonSound` → `PlaySoundOnClick`.
  - Ensure:
    - Each slider is set to the correct `AudioChannel`.
    - Each button uses the correct `SoundType`.

- **6.3 Remove unused ScriptableObjects and assets**
  - Identify any `UiSfxPlayer`/`GameMusicPlayer`/`IntroLoopAudioPlayer` assets that are no longer used directly.
  - Clean up orphaned audio components, mixer groups, and clips.

---

### Phase 7 – Testing & Validation

- **7.1 Functional smoke tests**
  - Validate on a representative set of scenes:
    - Game launches without missing reference errors.
    - All expected music plays (menu, each season, summary screens).
    - All critical SFX triggers still work (exhibits placed/picked, rarity reveal, UI buttons, narration previews).

- **7.2 Volume controls**
  - Confirm sliders:
    - Correctly reflect saved values across sessions.
    - Affect the intended channels only.
    - Play preview sounds (if configured) on pointer‑up.

- **7.3 Edge cases**
  - Verify behavior when:
    - AudioSystem is initialized from different entry scenes.
    - PlayerPrefs are cleared or corrupt (defaults are applied).
    - Rapid scene changes occur while music/SFX are playing.

- **7.4 Cleanup and docs**
  - Remove any remaining references to deprecated audio components.
  - Add brief README comments:
    - At the top of `AudioSystem`.
    - In `AudioConfig` asset inspector (description of how to use it).
  - Optionally, create a short “Audio How‑To” note in the design folder for future contributors.


