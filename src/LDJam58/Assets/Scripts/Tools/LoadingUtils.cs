using System.Collections.Generic;
using UnityEngine;

public static class LoadingUtils
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();
    private static Sprite _missingExhibitSprite;

    public static Sprite LoadSpriteOrDefault(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return GetMissingExhibitSprite();

        var fileFriendlyName = MakeFileFriendly(displayName);
        var resourcePath = $"Exh/Sprites/{fileFriendlyName}";

        if (SpriteCache.TryGetValue(resourcePath, out var cachedSprite))
            return cachedSprite;

        var sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            sprite = GetMissingExhibitSprite();
        }

        SpriteCache[resourcePath] = sprite;
        return sprite;
    }

    private static Sprite GetMissingExhibitSprite()
    {
        if (_missingExhibitSprite == null)
        {
            _missingExhibitSprite = Resources.Load<Sprite>("Exh/Sprites/MissingExhibitSprite");
            if (_missingExhibitSprite == null)
            {
                Debug.LogWarning("MissingExhibitSprite not found in Resources/Exh/Sprites/");
            }
        }
        return _missingExhibitSprite;
    }

    private static string MakeFileFriendly(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return "";

        var result = displayName.Replace(" ", "_")
                               .Replace("'", "")
                               .Replace("\"", "")
                               .Replace(",", "")
                               .Replace("(", "")
                               .Replace(")", "")
                               .Replace("[", "")
                               .Replace("]", "")
                               .Replace("{", "")
                               .Replace("}", "")
                               .Replace("!", "")
                               .Replace("?", "")
                               .Replace(":", "")
                               .Replace(";", "")
                               .Replace("/", "_")
                               .Replace("\\", "_")
                               .Replace(".", "_")
                               .Replace("-", "_");

        return result;
    }

    public static GameObject LoadPrefabOrDefault(string displayName, Vector2Int size)
    {
        if (string.IsNullOrEmpty(displayName))
            return GetFallbackPrefab(size);

        var fileFriendlyName = MakeFileFriendly(displayName);
        var resourcePath = $"Exh/Prefabs/{fileFriendlyName}";

        if (PrefabCache.TryGetValue(resourcePath, out var cachedPrefab))
            return cachedPrefab;

        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            prefab = GetFallbackPrefab(size);
        }

        PrefabCache[resourcePath] = prefab;
        return prefab;
    }

    private static GameObject GetFallbackPrefab(Vector2Int size)
    {
        var fallbackName = $"ExhTemp_{size.x}x{size.y}";
        var fallbackPath = $"Exh/Prefabs/{fallbackName}";

        if (PrefabCache.TryGetValue(fallbackPath, out var cachedFallback))
            return cachedFallback;

        var fallbackPrefab = Resources.Load<GameObject>(fallbackPath);
        if (fallbackPrefab == null)
        {
            Debug.LogWarning($"Fallback prefab {fallbackName} not found in Resources/Exh/Prefabs/");
        }

        PrefabCache[fallbackPath] = fallbackPrefab;
        return fallbackPrefab;
    }
}
