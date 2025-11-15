using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GlobalButtonSoundHook
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Handle initial scene load
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookupAllButtons();
    }

    private static void HookupAllButtons()
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            HookupButton(button);
        }
    }

    /// <summary>
    /// Manually hook up a button to the sound system. Useful for dynamically created buttons.
    /// </summary>
    public static void HookupButton(Button button)
    {
        if (button == null)
            return;

        // Skip if already has PlaySoundOnClick component
        if (button.GetComponent<PlaySoundOnClick>() != null)
            return;

        // Add the component - it will use default values (UIButtonClickPrimary sound)
        button.gameObject.AddComponent<PlaySoundOnClick>();
    }
}

