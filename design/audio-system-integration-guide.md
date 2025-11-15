## Audio System Integration & Legacy Removal Guide

This guide lists all actions needed to fully integrate the new code-first audio system and remove all legacy audio elements/files.

---

### 1. Configure `AudioConfig` and `AudioSystem`

- **1.1 Create the `AudioConfig` asset**
  - In Unity, create the config asset:
    - `Create → Audio → AudioConfig`.
  - Either:
    - Place it under `Assets/Resources/` and name it `AudioConfig` (so `AudioSystem` auto-loads it), or
    - Assign it directly to the `config` field on the `AudioSystem` component (if you place `AudioSystem` in a scene).

- **1.2 Set up channels**
  - Open the `AudioConfig` asset in the inspector.
  - Set **Mixer** to your main `AudioMixer`.
  - In **Channels**:
    - Add an entry for **`AudioChannel.Music`**:
      - **MixerParameterName**: `MusicVolume`
      - **MixerGroup**: the group your music currently uses.
      - **ReductionDb**: set to taste (0 is fine if you don’t need offset).
      - **PreviewClip**: optional clip to play when testing volume.
    - Add an entry for **`AudioChannel.Sound`**:
      - **MixerParameterName**: `SoundVolume`
      - **MixerGroup**: the group your SFX/UI sounds use.
      - **ReductionDb** and **PreviewClip** as desired.

- **1.3 Hook Introloop music**
  - In `AudioConfig`:
    - Assign your existing `IntroLoopAudioPlayer` asset to **IntroLoopPlayer**.
    - Fill **MusicTracksByIndex** with your `IntroloopAudio` tracks:
      - Index `0` = music for season/index 0, index `1` = season 1, etc.
    - Configure **Scene Music Mappings** (new feature):
      - For each scene that should have background music, add an entry:
        - **SceneName**: the exact scene name (e.g., `"MainMenu"`, `"GameScene"`, `"CreditsScene"`).
        - **Music**: the `IntroloopAudio` asset to play for that scene (drag directly from your project).
      - If a scene has no mapping, music will stop when entering that scene.
      - Example mappings:
        - `"MainMenu"` → `MainMenuMusic` (IntroloopAudio asset)
        - `"GameScene"` → `GameplayMusic` (IntroloopAudio asset, or leave unmapped to use season-based music via `PlaySeasonMusic` instead)
        - `"CreditsScene"` → `CreditsMusic` (IntroloopAudio asset)

- **1.4 Map sound effects**
  - Still in `AudioConfig`, configure **SoundDefinitions**:
    - For each `SoundType` you want to use (e.g. `ExhibitPickingBegan`, `SeasonSummaryVictory`, `PageTurn`, `Screenshot`, etc.):
      - Add an entry:
        - **Id**: the `SoundType` value.
        - **Clip**: the `AudioClip` to play.
        - **Volume**: volume level (0-1).
      - All sounds use the `AudioChannel.Sound` channel automatically (no need to specify).

---

### 2. Ensure `AudioSystem` bootstraps correctly

- **2.1 Use the built-in bootstrap**
  - The `AudioSystem` uses `[RuntimeInitializeOnLoadMethod]` to auto-create a persistent `"AudioSystem"` object.
  - Check in play mode that:
    - There is exactly **one** `AudioSystem` in the scene hierarchy.
    - It persists across scene loads.

- **2.2 Assign `AudioConfig` if needed**
  - If you did **not** put `AudioConfig` in `Resources/AudioConfig`:
    - Add an `AudioSystem` to a bootstrap scene (if not already present).
    - In the inspector, assign your `AudioConfig` asset to the `config` field.

---

### 3. Migrate music usage to `AudioSystem`

- **3.1 `PlaySeasonMusic`**
  - Ensure `PlaySeasonMusic` is present where needed (it now calls `AudioSystem.Instance.PlayMusicByIndex(CurrentGameState.ReadOnly.currentSeasonIndex)`).
  - Verify that `MusicTracksByIndex` in `AudioConfig` is ordered to match the season indices.

- **3.2 Configure scene-based music (no scripts needed)**
  - The `AudioSystem` now automatically plays music when scenes load based on **Scene Music Mappings** in `AudioConfig`.
  - For each scene in your project:
    - Check if it currently uses:
      - `SceneBackgroundMusic`
      - `IntroLoopSceneBackgroundMusic`
      - `IntroLoopMusicPlaylist`
      - `InitGameMusicPlayer`
      - `InitIntroLoopAudioPlayer`
    - Determine what music it should play:
      - **Fixed track**: Add a mapping in `AudioConfig` → **Scene Music Mappings**:
        - SceneName = the scene's exact name (check in Build Settings or scene file name).
        - Music = the `IntroloopAudio` asset to play for that scene.
      - **Season-based**: Keep `PlaySeasonMusic` component (it calls `AudioSystem.Instance.PlayMusicByIndex(...)` which will override scene music).
      - **No music**: Leave unmapped (music will stop automatically).
  - **No per-scene scripts needed** — `AudioSystem` handles scene loading automatically via `SceneManager.sceneLoaded`.
  - After confirming music plays correctly:
    - **Remove these components from all scenes**:
      - `SceneBackgroundMusic`
      - `IntroLoopSceneBackgroundMusic`
      - `IntroLoopMusicPlaylist` (if no longer needed anywhere)
      - `InitGameMusicPlayer`
      - `InitIntroLoopAudioPlayer`

---

### 4. Migrate volume sliders and preview sounds

- **4.1 Replace `MixerVolumeSlider` with `AudioVolumeSlider`**
  - For each scene/prefab using `MixerVolumeSlider`:
    - Note which parameter it controls (`MusicVolume` or `SoundVolume`).
    - Remove the `MixerVolumeSlider` component.
    - Add an `AudioVolumeSlider` component:
      - Set **channel** =
        - `AudioChannel.Music` for music volume sliders.
        - `AudioChannel.Sound` for SFX sliders.
      - If the `Slider` is on the same GameObject, you can leave `slider` unassigned (the script will auto-find it).

- **4.2 Configure slider pointer-up preview**
  - For sliders where you want a preview sound:
    - Ensure `AudioVolumeSlider.playPreviewOnPointerUp` is enabled.
    - Wire a UI event (e.g., using `EventTrigger` or the slider’s pointer-up event) to call `AudioVolumeSlider.OnPointerUp()`.

- **4.3 Remove old volume init/preview scripts**
  - Once all sliders use `AudioVolumeSlider` and work correctly:
    - Remove these components from all scenes/prefabs:
      - `InitAudioVolumeLevel`
      - `OnVolumeChangedSound`
      - `MixerVolumeSlider`
    - After verifying that there are no remaining references, delete these scripts:
      - `z_CoreLib/Audio/MixerVolumeSlider.cs`
      - `z_CoreLib/Audio/MixerVolumeChanged.cs`
      - `z_CoreLib/Audio/InitAudioVolumeLevel.cs`
      - `z_CoreLib/Audio/OnVolumeChangedSound.cs`

---

### 5. Migrate UI button sounds and other SFX to `AudioSystem`

- **5.1 Replace `OnClickPlayUiSound` and `ButtonSound`**
  - Search project for **`OnClickPlayUiSound`** and **`ButtonSound`**.
  - For each button GameObject:
    - Remove `OnClickPlayUiSound` or `ButtonSound`.
    - Add `PlaySoundOnClick`:
      - Set **sound** to a `SoundType` configured in `AudioConfig`.
      - Set **useUiRectPanning**:
        - `true` for UI sounds that should pan based on position.
        - `false` for center/global button SFX.

- **5.2 Convert `Pages` to `AudioSystem`**
  - In `Pages`:
    - Remove the `UiSfxPlayer player` serialized field.
    - Add or choose a `SoundType` (e.g. `PageTurn`), and configure it in `AudioConfig` with the `pageSound` clip.
    - Change:
      - `player.Play(pageSound);` in `MoveNext` and `MovePrevious` to:
        - `AudioSystem.Instance.Play(SoundType.PageTurn, default);`

- **5.3 Convert `ScreenRecorder` to `AudioSystem`**
  - In `ScreenRecorder`:
    - Remove the `UiSfxPlayer soundPlayer` field.
    - Add or choose a `SoundType` for screenshot capture (e.g. `Screenshot`), and map it in `AudioConfig`.
    - In `Update()`, where screenshots are taken:
      - Replace:
        - `if (soundPlayer != null && screenshotSound != null) soundPlayer.Play(screenshotSound);`
      - With:
        - `AudioSystem.Instance.Play(SoundType.Screenshot, default);`

- **5.4 Replace `SoundGuy` + `PlaySoundRequested` usage**
  - Option A (recommended: direct `AudioSystem` calls, no messages):
    - In `ExhibitPickerView`:
      - `PlayRaritySound`:
        - Replace:
          - `Message.Publish(new PlaySoundRequested(GetSoundTypeForRarity(_rarity), uiRect, null));`
        - With:
          - `AudioSystem.Instance.PlayAtUIRect(GetSoundTypeForRarity(_rarity), uiRect);`
      - `PickExhibit`:
        - Replace:
          - `Message.Publish(new PlaySoundRequested(SoundType.ExhibitPicked, uiRect, null));`
        - With:
          - `AudioSystem.Instance.PlayAtUIRect(SoundType.ExhibitPicked, uiRect);`
    - In `SeasonSummaryScreenV2`:
      - For the scoring-began and result callbacks:
        - Replace `Message.Publish(new PlaySoundRequested(..., rectTransform));`
        - With `AudioSystem.Instance.PlayAtUIRect(SoundType.Whatever, rectTransform);`
    - For any other places that publish `PlaySoundRequested` or rely on `SoundGuy`:
      - Replace the publish with the equivalent `AudioSystem.Instance.Play(...)`/`PlayAtUIRect(...)` call.
  - After all these changes:
    - Remove the `SoundGuy` component from all scenes.
    - Delete the `Scripts/Audio/SoundGuy.cs` file (which also defines `PlaySoundRequested`) once there are no references left.

- **5.5 Remove `UiSfxPlayer` and its initializer**
  - Once `Pages`, `ScreenRecorder`, all buttons, and any other SFX are using `AudioSystem`:
    - Remove `InitUiSfxPlayer` components from scenes.
    - Delete any `UiSfxPlayer` ScriptableObject assets from the project.
    - Delete scripts:
      - `z_CoreLib/Audio/UiSfxPlayer.cs`
      - `z_CoreLib/Audio/InitUiSfxPlayer.cs`
      - `z_CoreLib/UI/OnClickPlayUiSound.cs`
      - `z_CoreLib/UI/ButtonSound.cs`

---

### 6. Final script and asset cleanup

- **6.1 Remove old music helpers**
  - Once all music is played via `AudioSystem.PlayMusicByIndex` and `AudioConfig`:
    - Delete scripts:
      - `z_CoreLib/Audio/GameMusicPlayer.cs`
      - `z_CoreLib/Audio/SceneBackgroundMusic.cs`
      - `z_CoreLib/Audio/InitGameMusicPlayer.cs`
      - `PluginIntegrations/Introloop/IntroLoopSceneBackgroundMusic.cs`
      - `PluginIntegrations/Introloop/InitIntroLoopAudioPlayer.cs`
      - `PluginIntegrations/Introloop/IntroLoopMusicPlaylist.cs` (if no longer referenced).
  - Keep the **Introloop plugin runtime** and **Editor** scripts (under `Assets/Plugins/Introloop/`) — those are third-party.

- **6.2 Verify no remaining references**
  - In Unity, use “Search in Project” / inspector reference checks to confirm there are **no references** to:
    - `UiSfxPlayer`
    - `InitUiSfxPlayer`
    - `GameMusicPlayer`
    - `SceneBackgroundMusic`
    - `InitGameMusicPlayer`
    - `MixerVolumeSlider`
    - `InitAudioVolumeLevel`
    - `OnVolumeChangedSound`
    - `MixerVolumeChanged`
    - `OnClickPlayUiSound`
    - `ButtonSound`
    - `SoundGuy`
    - `PlaySoundRequested`
    - `IntroLoopSceneBackgroundMusic`
    - `InitIntroLoopAudioPlayer`
    - `IntroLoopMusicPlaylist` (if you removed it).
  - Only delete a script once its usage count is 0.

---

### 7. Testing checklist

- **7.1 Startup & scenes**
  - Start the game from all entry points (main menu, gameplay, etc.).
  - Confirm:
    - Exactly one `AudioSystem` exists and persists across scenes.
    - Music plays where expected in each scene (menu, seasons, summary).

- **7.2 Controls & SFX**
  - Test volume sliders:
    - Music and sound sliders change audible volume correctly.
    - Volume values persist across restarts.
    - If configured, preview sounds play when releasing the slider.
  - Test UI interactions:
    - Buttons that previously had click sounds still do via `PlaySoundOnClick`.
    - Exhibit rarity, pick, and season summary sounds trigger correctly using `AudioSystem`.

- **7.3 Final cleanup**
  - If all behavior matches or improves on the old system:
    - Commit your changes with a message indicating:
      - New `AudioSystem` + `AudioConfig` are in use.
      - All legacy audio components have been removed.


